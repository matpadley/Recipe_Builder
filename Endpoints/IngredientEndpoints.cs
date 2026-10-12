using RecipeIngredients.Models;
using RecipeIngredients.Services;

namespace RecipeIngredients.Endpoints;

public static class IngredientEndpoints
{
    public static void MapIngredientEndpoints(this IEndpointRouteBuilder api)
    {
        var ingredients = api.MapGroup("/ingredients");

        ingredients.MapGet("/", (IIngredientStore store) => store.GetAllAsync());

        ingredients.MapPost("/", async (Ingredient ingredient, IIngredientStore store) =>
        {
            await store.AddAsync(ingredient);
            return Results.Created($"/api/ingredients/{ingredient.Id}", ingredient);
        });

        ingredients.MapPut("/{id:int}", async (int id, Ingredient ingredient, IIngredientStore store) =>
        {
            ingredient.Id = id;
            return await store.UpdateAsync(ingredient) ? Results.Ok(ingredient) : Results.NotFound();
        });

        ingredients.MapDelete("/{id:int}", async (int id, IIngredientStore store) =>
            await store.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

        // Deducts the given amounts from stock, same rules as "I made this".
        ingredients.MapPost("/use", (List<RecipeIngredient> used, IIngredientStore store) =>
            store.UseAsync(used));
    }
}
