namespace RecipeIngredients.Endpoints;

/// <summary>
/// JSON API over the ingredient, staple and recipe stores, so Claude can read and
/// update the data when the app runs on another machine. No auth by design —
/// the app only runs on a local network.
/// </summary>
public static class ApiEndpoints
{
    public static void MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        api.MapIngredientEndpoints();
        api.MapStapleEndpoints();
        api.MapRecipeEndpoints();
    }
}
