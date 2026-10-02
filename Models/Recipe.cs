namespace RecipeIngredients.Models;

public class Recipe
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? Servings { get; set; }

    public int? PrepMinutes { get; set; }

    public int? CookMinutes { get; set; }

    public List<RecipeIngredient> Ingredients { get; set; } = [];

    public List<string> Steps { get; set; } = [];

    public List<string> Tags { get; set; } = [];

    public string? Notes { get; set; }

    public bool IsFavourite { get; set; }

    public DateTime? LastUsedUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
