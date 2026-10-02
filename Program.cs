using MudBlazor.Services;
using RecipeIngredients.Components;
using RecipeIngredients.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

var ingredientsFile = builder.Configuration["IngredientsFile"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "ingredients.json");
builder.Services.AddSingleton(new IngredientStore(ingredientsFile));

var recipesFile = builder.Configuration["RecipesFile"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "recipes.json");
builder.Services.AddSingleton(new RecipeStore(recipesFile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
