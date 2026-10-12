using System.Text.Json;
using System.Text.Json.Serialization;
using RecipeIngredients.Models;

namespace RecipeIngredients.Services;

/// <summary>
/// Persists kitchen staples to a single JSON file, the same way as
/// <see cref="IngredientStore"/>.
/// </summary>
public class StapleStore(string filePath) : IStapleStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<List<Staple>> GetAllAsync()
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

    public async Task AddAsync(Staple staple)
    {
        await _lock.WaitAsync();
        try
        {
            var staples = await ReadAsync();
            staple.Id = staples.Count == 0 ? 1 : staples.Max(s => s.Id) + 1;
            staples.Add(staple);
            await WriteAsync(staples);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> UpdateAsync(Staple updated)
    {
        await _lock.WaitAsync();
        try
        {
            var staples = await ReadAsync();
            var existing = staples.FirstOrDefault(s => s.Id == updated.Id);
            if (existing is null)
            {
                return false;
            }

            existing.Name = updated.Name;
            existing.Available = updated.Available;
            await WriteAsync(staples);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task ReplaceAllAsync(List<Staple> staples)
    {
        await _lock.WaitAsync();
        try
        {
            await WriteAsync(staples);
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
            var staples = await ReadAsync();
            if (staples.RemoveAll(s => s.Id == id) == 0)
            {
                return false;
            }

            await WriteAsync(staples);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<Staple>> ReadAsync()
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<List<Staple>>(stream, JsonOptions) ?? [];
    }

    private async Task WriteAsync(List<Staple> staples)
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
            await JsonSerializer.SerializeAsync(stream, staples, JsonOptions);
        }

        File.Move(tempPath, filePath, overwrite: true);
    }
}
