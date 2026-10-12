using RecipeIngredients.Models;
using RecipeIngredients.Services;

namespace RecipeIngredients.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this IEndpointRouteBuilder api)
    {
        var recipes = api.MapGroup("/recipes");

        recipes.MapGet("/", (IRecipeStore store) => store.GetAllAsync());

        recipes.MapGet("/{id:int}", async (int id, IRecipeStore store) =>
            await store.GetAsync(id) is { } recipe ? Results.Ok(recipe) : Results.NotFound());

        recipes.MapPost("/", async (Recipe recipe, IRecipeStore store) =>
        {
            if (string.IsNullOrWhiteSpace(recipe.Title) || recipe.Ingredients.Count == 0 || recipe.Steps.Count == 0)
            {
                return Results.BadRequest("title, ingredients and steps are required");
            }

            await store.AddAsync(recipe);
            return Results.Created($"/api/recipes/{recipe.Id}", recipe);
        });

        recipes.MapDelete("/{id:int}", async (int id, IRecipeStore store) =>
            await store.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

        // Same as the "I made this" button: deducts ingredients and records last used.
        recipes.MapPost("/{id:int}/made", async (int id, IRecipeStore store, IIngredientStore ingredientStore) =>
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
