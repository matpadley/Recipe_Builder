using System.ComponentModel.DataAnnotations;

namespace RecipeIngredients.Models;

/// <summary>
/// A kitchen staple (spice, oil, sauce, etc.) that's tracked only by whether
/// it's on hand — no amounts or units.
/// </summary>
public class Staple
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    // Defaults to true, so a new staple is on hand unless marked as run out.
    public bool Available { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
