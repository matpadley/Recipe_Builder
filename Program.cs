using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RecipeIngredients.Data;
using RecipeIngredients.Endpoints;
using RecipeIngredients.Models;
using RecipeIngredients.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddKeyPerFile("/run/secrets/", optional: true);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Set ConnectionStrings:Default.");
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
builder.Services.AddScoped<IIngredientStore, DbIngredientStore>();
builder.Services.AddScoped<IRecipeStore, DbRecipeStore>();
builder.Services.AddScoped<IStapleStore, DbStapleStore>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await db.Database.ExecuteSqlRawAsync(
        "ALTER TABLE \"Ingredients\" ADD COLUMN IF NOT EXISTS \"Canned\" boolean NOT NULL DEFAULT false");
    await db.Database.ExecuteSqlRawAsync(
        "ALTER TABLE \"Recipes\" ADD COLUMN IF NOT EXISTS \"Nutrition\" jsonb");
    await SeedFromJsonAsync(db, app.Configuration, app.Environment);
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
app.MapGet("/health", () => Results.Ok("ok")).AddEndpointFilter<ApiKeyFilter>();
app.MapApiEndpoints();

app.Run();

static async Task SeedFromJsonAsync(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
{
    if (await db.Ingredients.AnyAsync() || await db.Staples.AnyAsync() || await db.Recipes.AnyAsync())
        return;

    var opts = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    var ingredientsFile = config["IngredientsFile"]
        ?? Path.Combine(env.ContentRootPath, "ingredients.json");
    if (File.Exists(ingredientsFile))
    {
        await using var s = File.OpenRead(ingredientsFile);
        var list = await JsonSerializer.DeserializeAsync<List<Ingredient>>(s, opts) ?? [];
        foreach (var i in list) i.Id = 0;
        db.Ingredients.AddRange(list);
    }

    var staplesFile = config["StaplesFile"]
        ?? Path.Combine(env.ContentRootPath, "staples.json");
    if (File.Exists(staplesFile))
    {
        await using var s = File.OpenRead(staplesFile);
        var list = await JsonSerializer.DeserializeAsync<List<Staple>>(s, opts) ?? [];
        foreach (var x in list) x.Id = 0;
        db.Staples.AddRange(list);
    }

    var recipesFile = config["RecipesFile"]
        ?? Path.Combine(env.ContentRootPath, "recipes.json");
    if (File.Exists(recipesFile))
    {
        await using var s = File.OpenRead(recipesFile);
        var list = await JsonSerializer.DeserializeAsync<List<Recipe>>(s, opts) ?? [];
        foreach (var r in list) r.Id = 0;
        db.Recipes.AddRange(list);
    }

    await db.SaveChangesAsync();
}
