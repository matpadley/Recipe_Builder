using RecipeIngredients.Models;
using RecipeIngredients.Services;

namespace RecipeIngredients.Endpoints;

public static class IngredientEndpoints
{
    public static void MapIngredientEndpoints(this IEndpointRouteBuilder api)
    {
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
    }
}
