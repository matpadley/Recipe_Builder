using RecipeIngredients.Models;

namespace RecipeIngredients.Components.Pages;

internal static class RecipeTime
{
    public static int? Total(Recipe recipe) =>
        recipe.PrepMinutes is null && recipe.CookMinutes is null
            ? null
            : (recipe.PrepMinutes ?? 0) + (recipe.CookMinutes ?? 0);

    public static string Format(int minutes) =>
        minutes < 60 ? $"{minutes} min"
        : minutes % 60 == 0 ? $"{minutes / 60} h"
        : $"{minutes / 60} h {minutes % 60} min";
}
