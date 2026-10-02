using System.Text.Json;
using System.Text.Json.Serialization;
using RecipeIngredients.Models;

namespace RecipeIngredients.Services;

/// <summary>
/// Reads saved recipes from a single JSON file. Recipes are written to the file
/// by the save-recipe Claude skill; the app lists, shows, and deletes them, and
/// tracks favourites and when each was last used.
/// </summary>
public class RecipeStore(string filePath)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<List<Recipe>> GetAllAsync()
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

    public async Task<Recipe?> GetAsync(int id)
    {
        await _lock.WaitAsync();
        try
        {
            return (await ReadAsync()).FirstOrDefault(r => r.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetFavouriteAsync(int id, bool isFavourite)
    {
        await UpdateAsync(id, r => r.IsFavourite = isFavourite);
    }

    public async Task MarkUsedAsync(int id)
    {
        await UpdateAsync(id, r => r.LastUsedUtc = DateTime.UtcNow);
    }

    private async Task UpdateAsync(int id, Action<Recipe> change)
    {
        await _lock.WaitAsync();
        try
        {
            var recipes = await ReadAsync();
            var recipe = recipes.FirstOrDefault(r => r.Id == id);
            if (recipe is null)
            {
                return;
            }

            change(recipe);
            await WriteAsync(recipes);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteAsync(int id)
    {
        await _lock.WaitAsync();
        try
        {
            var recipes = await ReadAsync();
            if (recipes.RemoveAll(r => r.Id == id) > 0)
            {
                await WriteAsync(recipes);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<Recipe>> ReadAsync()
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<List<Recipe>>(stream, JsonOptions) ?? [];
    }

    private async Task WriteAsync(List<Recipe> recipes)
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
            await JsonSerializer.SerializeAsync(stream, recipes, JsonOptions);
        }

        File.Move(tempPath, filePath, overwrite: true);
    }
}
