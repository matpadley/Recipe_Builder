using RecipeIngredients.Models;

namespace RecipeIngredients.Services;

public interface IRecipeStore
{
    Task<List<Recipe>> GetAllAsync();
    Task<Recipe?> GetAsync(int id);
    Task AddAsync(Recipe recipe);
    Task SetFavouriteAsync(int id, bool isFavourite);
    Task MarkUsedAsync(int id);
    Task ReplaceAllAsync(List<Recipe> recipes);
    Task<bool> DeleteAsync(int id);
}
