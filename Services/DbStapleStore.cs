using Microsoft.EntityFrameworkCore;
using RecipeIngredients.Data;
using RecipeIngredients.Models;

namespace RecipeIngredients.Services;

public class DbStapleStore(AppDbContext db) : IStapleStore
{
    public Task<List<Staple>> GetAllAsync() =>
        db.Staples.ToListAsync();

    public async Task AddAsync(Staple staple)
    {
        staple.Id = 0;
        db.Staples.Add(staple);
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Staple updated)
    {
        var existing = await db.Staples.FindAsync(updated.Id);
        if (existing is null) return false;
        existing.Name = updated.Name;
        existing.Available = updated.Available;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task ReplaceAllAsync(List<Staple> staples)
    {
        await db.Staples.ExecuteDeleteAsync();
        foreach (var s in staples) s.Id = 0;
        db.Staples.AddRange(staples);
        await db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var staple = await db.Staples.FindAsync(id);
        if (staple is null) return false;
        db.Staples.Remove(staple);
        await db.SaveChangesAsync();
        return true;
    }
}
