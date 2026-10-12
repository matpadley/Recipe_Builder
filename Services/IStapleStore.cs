using RecipeIngredients.Models;

namespace RecipeIngredients.Services;

public interface IStapleStore
{
    Task<List<Staple>> GetAllAsync();
    Task AddAsync(Staple staple);
    Task<bool> UpdateAsync(Staple updated);
    Task ReplaceAllAsync(List<Staple> staples);
    Task<bool> DeleteAsync(int id);
}
