using RecipeIngredients.Models;

namespace RecipeIngredients.Services;

public interface IIngredientStore
{
    Task<List<Ingredient>> GetAllAsync();
    Task AddAsync(Ingredient ingredient);
    Task<bool> UpdateAsync(Ingredient updated);
    Task<List<UsageLine>> UseAsync(IEnumerable<RecipeIngredient> used);
    Task ReplaceAllAsync(List<Ingredient> ingredients);
    Task<bool> DeleteAsync(int id);
}
