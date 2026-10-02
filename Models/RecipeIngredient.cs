namespace RecipeIngredients.Models;

/// <summary>
/// An ingredient line in a saved recipe. Amount and unit are optional so lines
/// like "salt, to taste" can be stored with just a name and a note.
/// </summary>
public class RecipeIngredient
{
    public string Name { get; set; } = string.Empty;

    public decimal? Amount { get; set; }

    public MeasurementUnit? Unit { get; set; }

    public string? Note { get; set; }
}
