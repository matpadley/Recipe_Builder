using RecipeIngredients.Models;
using RecipeIngredients.Services;

namespace RecipeIngredients.Endpoints;

public static class StapleEndpoints
{
    public static void MapStapleEndpoints(this IEndpointRouteBuilder api)
    {
        var staples = api.MapGroup("/staples");

        staples.MapGet("/", (StapleStore store) => store.GetAllAsync());

        staples.MapPost("/", async (Staple staple, StapleStore store) =>
        {
            if (string.IsNullOrWhiteSpace(staple.Name))
            {
                return Results.BadRequest("name is required");
            }

            await store.AddAsync(staple);
            return Results.Created($"/api/staples/{staple.Id}", staple);
        });

        staples.MapPut("/{id:int}", async (int id, Staple staple, StapleStore store) =>
        {
            staple.Id = id;
            return await store.UpdateAsync(staple) ? Results.Ok(staple) : Results.NotFound();
        });

        staples.MapDelete("/{id:int}", async (int id, StapleStore store) =>
            await store.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());
    }
}
