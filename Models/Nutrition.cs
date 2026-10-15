namespace RecipeIngredients.Models;

/// <summary>
/// Estimated nutritional values for one serving of a recipe.
/// </summary>
public class Nutrition
{
    public int Kcal { get; set; }

    public decimal ProteinG { get; set; }

    public decimal CarbsG { get; set; }
}
