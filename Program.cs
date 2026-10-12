using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using RecipeIngredients.Components;
using RecipeIngredients.Data;
using RecipeIngredients.Endpoints;
using RecipeIngredients.Models;
using RecipeIngredients.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddKeyPerFile("/run/secrets/", optional: true);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var useDatabase = builder.Configuration.GetValue<bool>("UseDatabase");

if (useDatabase)
{
    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Set ConnectionStrings:Default in appsettings.json.");
    builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
    builder.Services.AddScoped<IIngredientStore, DbIngredientStore>();
    builder.Services.AddScoped<IRecipeStore, DbRecipeStore>();
    builder.Services.AddScoped<IStapleStore, DbStapleStore>();
}
else
{
    var ingredientsFile = builder.Configuration["IngredientsFile"]
        ?? Path.Combine(builder.Environment.ContentRootPath, "ingredients.json");
    builder.Services.AddSingleton<IIngredientStore>(new IngredientStore(ingredientsFile));

    var recipesFile = builder.Configuration["RecipesFile"]
        ?? Path.Combine(builder.Environment.ContentRootPath, "recipes.json");
    builder.Services.AddSingleton<IRecipeStore>(new RecipeStore(recipesFile));

    var staplesFile = builder.Configuration["StaplesFile"]
        ?? Path.Combine(builder.Environment.ContentRootPath, "staples.json");
    builder.Services.AddSingleton<IStapleStore>(new StapleStore(staplesFile));
}

var app = builder.Build();

if (useDatabase)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedFromJsonAsync(db, app.Configuration, app.Environment);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapApiEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

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
