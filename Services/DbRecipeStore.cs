using Microsoft.EntityFrameworkCore;
using RecipeIngredients.Data;
using RecipeIngredients.Models;

namespace RecipeIngredients.Services;

public class DbRecipeStore(AppDbContext db) : IRecipeStore
{
    public Task<List<Recipe>> GetAllAsync() =>
        db.Recipes.ToListAsync();

    public async Task<Recipe?> GetAsync(int id) =>
        await db.Recipes.FindAsync(id);

    public async Task AddAsync(Recipe recipe)
    {
        recipe.Id = 0;
        db.Recipes.Add(recipe);
        await db.SaveChangesAsync();
    }

    public async Task SetFavouriteAsync(int id, bool isFavourite)
    {
        var recipe = await db.Recipes.FindAsync(id);
        if (recipe is null) return;
        recipe.IsFavourite = isFavourite;
        await db.SaveChangesAsync();
    }

    public async Task MarkUsedAsync(int id)
    {
        var recipe = await db.Recipes.FindAsync(id);
        if (recipe is null) return;
        recipe.LastUsedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task ReplaceAllAsync(List<Recipe> recipes)
    {
        await db.Recipes.ExecuteDeleteAsync();
        foreach (var r in recipes) r.Id = 0;
        db.Recipes.AddRange(recipes);
        await db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var recipe = await db.Recipes.FindAsync(id);
        if (recipe is null) return false;
        db.Recipes.Remove(recipe);
        await db.SaveChangesAsync();
        return true;
    }
}
