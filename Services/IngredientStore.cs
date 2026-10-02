using System.Text.Json;
using System.Text.Json.Serialization;
using RecipeIngredients.Models;

namespace RecipeIngredients.Services;

/// <summary>
/// Persists ingredients to a single JSON file so they can be read directly
/// (e.g. by Claude) without going through the app.
/// </summary>
public class IngredientStore(string filePath)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<List<Ingredient>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return await ReadAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(Ingredient ingredient)
    {
        await _lock.WaitAsync();
        try
        {
            var ingredients = await ReadAsync();
            ingredient.Id = ingredients.Count == 0 ? 1 : ingredients.Max(i => i.Id) + 1;
            ingredients.Add(ingredient);
            await WriteAsync(ingredients);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> UpdateAsync(Ingredient updated)
    {
        await _lock.WaitAsync();
        try
        {
            var ingredients = await ReadAsync();
            var existing = ingredients.FirstOrDefault(i => i.Id == updated.Id);
            if (existing is null)
            {
                return false;
            }

            existing.Name = updated.Name;
            existing.Amount = updated.Amount;
            existing.Unit = updated.Unit;
            existing.Available = updated.Available;
            await WriteAsync(ingredients);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Deducts the given recipe ingredients from stock and saves the result.
    /// </summary>
    public async Task<List<UsageLine>> UseAsync(IEnumerable<RecipeIngredient> used)
    {
        await _lock.WaitAsync();
        try
        {
            var ingredients = await ReadAsync();
            var lines = IngredientUsage.Apply(ingredients, used);
            await WriteAsync(ingredients);
            return lines;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _lock.WaitAsync();
        try
        {
            var ingredients = await ReadAsync();
            if (ingredients.RemoveAll(i => i.Id == id) == 0)
            {
                return false;
            }

            await WriteAsync(ingredients);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<Ingredient>> ReadAsync()
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<List<Ingredient>>(stream, JsonOptions) ?? [];
    }

    private async Task WriteAsync(List<Ingredient> ingredients)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Write to a temp file then swap, so a crash mid-write can't corrupt the file.
        var tempPath = filePath + ".tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, ingredients, JsonOptions);
        }

        File.Move(tempPath, filePath, overwrite: true);
    }
}
