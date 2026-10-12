using Microsoft.EntityFrameworkCore;
using RecipeIngredients.Data;
using RecipeIngredients.Models;

namespace RecipeIngredients.Services;

public class DbIngredientStore(AppDbContext db) : IIngredientStore
{
    public Task<List<Ingredient>> GetAllAsync() =>
        db.Ingredients.ToListAsync();

    public async Task AddAsync(Ingredient ingredient)
    {
        ingredient.Id = 0;
        db.Ingredients.Add(ingredient);
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Ingredient updated)
    {
        var existing = await db.Ingredients.FindAsync(updated.Id);
        if (existing is null) return false;

        existing.Name = updated.Name;
        existing.Amount = updated.Amount;
        existing.Unit = updated.Unit;
        existing.Available = updated.Available;
        existing.Frozen = updated.Frozen;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<UsageLine>> UseAsync(IEnumerable<RecipeIngredient> used)
    {
        var stock = await db.Ingredients.ToListAsync();
        var originalSet = stock.ToHashSet();

        var lines = IngredientUsage.Apply(stock, used);

        foreach (var removed in originalSet.Except(stock))
            db.Ingredients.Remove(removed);

        foreach (var added in stock.Except(originalSet))
        {
            added.Id = 0;
            db.Ingredients.Add(added);
        }

        await db.SaveChangesAsync();
        return lines;
    }

    public async Task ReplaceAllAsync(List<Ingredient> ingredients)
    {
        await db.Ingredients.ExecuteDeleteAsync();
        foreach (var i in ingredients) i.Id = 0;
        db.Ingredients.AddRange(ingredients);
        await db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ingredient = await db.Ingredients.FindAsync(id);
        if (ingredient is null) return false;
        db.Ingredients.Remove(ingredient);
        await db.SaveChangesAsync();
        return true;
    }
}
