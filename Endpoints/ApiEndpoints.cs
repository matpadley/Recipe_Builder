namespace RecipeIngredients.Endpoints;

/// <summary>
/// JSON API over the ingredient, staple and recipe stores, so Claude can read and
/// update the data when the app runs on another machine, and so the unified UI
/// can use this app as a microservice. Every /api call needs the X-Api-Key header.
/// </summary>
public static class ApiEndpoints
{
    public static void MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").AddEndpointFilter<ApiKeyFilter>();

        api.MapIngredientEndpoints();
        api.MapStapleEndpoints();
        api.MapRecipeEndpoints();
    }
}
