using RecipeIngredients.Models;
using RecipeIngredients.Services;

namespace RecipeIngredients.Endpoints;

/// <summary>
/// JSON API over the ingredient and recipe stores, so Claude can read and
/// update the data when the app runs on another machine. No auth by design —
/// the app only runs on a local network.
/// </summary>
public static class ApiEndpoints
{
    public static void MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        var ingredients = api.MapGroup("/ingredients");

        ingredients.MapGet("/", (IngredientStore store) => store.GetAllAsync());

        ingredients.MapPost("/", async (Ingredient ingredient, IngredientStore store) =>
        {
            await store.AddAsync(ingredient);
            return Results.Created($"/api/ingredients/{ingredient.Id}", ingredient);
        });

        ingredients.MapPut("/{id:int}", async (int id, Ingredient ingredient, IngredientStore store) =>
        {
            ingredient.Id = id;
            return await store.UpdateAsync(ingredient) ? Results.Ok(ingredient) : Results.NotFound();
        });

        ingredients.MapDelete("/{id:int}", async (int id, IngredientStore store) =>
            await store.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

        // Deducts the given amounts from stock, same rules as "I made this".
        ingredients.MapPost("/use", (List<RecipeIngredient> used, IngredientStore store) =>
            store.UseAsync(used));

        var recipes = api.MapGroup("/recipes");

        recipes.MapGet("/", (RecipeStore store) => store.GetAllAsync());

        recipes.MapGet("/{id:int}", async (int id, RecipeStore store) =>
            await store.GetAsync(id) is { } recipe ? Results.Ok(recipe) : Results.NotFound());

        recipes.MapPost("/", async (Recipe recipe, RecipeStore store) =>
        {
            if (string.IsNullOrWhiteSpace(recipe.Title) || recipe.Ingredients.Count == 0 || recipe.Steps.Count == 0)
            {
                return Results.BadRequest("title, ingredients and steps are required");
            }

            await store.AddAsync(recipe);
            return Results.Created($"/api/recipes/{recipe.Id}", recipe);
        });

        recipes.MapDelete("/{id:int}", async (int id, RecipeStore store) =>
            await store.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

        // Same as the "I made this" button: deducts ingredients and records last used.
        recipes.MapPost("/{id:int}/made", async (int id, RecipeStore store, IngredientStore ingredientStore) =>
        {
            if (await store.GetAsync(id) is not { } recipe)
            {
                return Results.NotFound();
            }

            var lines = await ingredientStore.UseAsync(recipe.Ingredients);
            await store.MarkUsedAsync(id);
            return Results.Ok(lines);
        });
    }
}
