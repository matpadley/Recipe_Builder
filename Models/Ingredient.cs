using System.ComponentModel.DataAnnotations;

namespace RecipeIngredients.Models;

public class Ingredient
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal Amount { get; set; }

    public MeasurementUnit Unit { get; set; }

    [Range(1, 20)]
    public int Available { get; set; }

    // Defaults to false, so older JSON without "frozen" still loads.
    public bool Frozen { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
