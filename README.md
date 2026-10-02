# Recipe Ingredients

A Blazor Server app for tracking the ingredients you have on hand. Ingredients
are saved to a plain JSON file (`ingredients.json`) that Claude can read
directly to suggest recipes — the app itself makes no API calls.

## What it does

- **Ingredients** — add, edit, and delete the ingredients you have available,
  with an amount, unit, and quantity on hand.
- **Recipes** — ask Claude (e.g. in Claude Code, from this folder) for a recipe
  using the ingredients in `ingredients.json`. See [CLAUDE.md](CLAUDE.md).
- **Saved Recipes** — run `/save-recipe` in Claude Code to save a recipe to
  `recipes.json`; saved recipes are listed at `/recipes` in the app with
  servings, total time, and tags. You can view, favourite (favourites sort
  first), or delete them.
- **"I made this"** — on a recipe's page, deducts its ingredients from
  `ingredients.json` and records when you last made it. Names are matched
  case-insensitively (ignoring plurals) and units are converted within the
  same kind (g/kg, ml/l/tsp/tbsp/cup, pcs).

> [!NOTE]
> Partly used items keep their pack size: 2 × 400 g minus 200 g leaves
> 1 × 400 g plus a separate 1 × 200 g entry. Recipe ingredients with no
> quantity, no matching stock, or incompatible units are skipped and listed
> in the result.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Running locally

```bash
dotnet restore
dotnet run
```

The app starts at `http://localhost:5122` (see
[Properties/launchSettings.json](Properties/launchSettings.json) for the
HTTPS profile). Ingredients are written to `ingredients.json` in the project
folder, which is created when you add the first ingredient.

## Running with Docker

```bash
docker build -t recipe-ingredients .
docker run -p 8080:8080 -v "$PWD:/app/data" recipe-ingredients
```

The container listens on port 8080 and reads/writes `ingredients.json` and
`recipes.json` in `/app/data`. Bind-mounting the project folder there, as
above, puts the files where Claude can read and write them.

## Tech stack

- ASP.NET Core Blazor Server (.NET 10, interactive server render mode)
- [MudBlazor](https://mudblazor.com/) for UI components
- JSON file storage via `System.Text.Json`

## Project structure

```
Components/
  Layout/       Main layout and nav menu
  Pages/        Ingredients, Recipes, RecipeDetail, Error, NotFound
  Dialogs/      Add/edit ingredient dialog
Models/         Ingredient, MeasurementUnit, Recipe, RecipeIngredient
Services/
  IngredientStore, RecipeStore   JSON file persistence
  IngredientUsage                Deducts a recipe's ingredients from stock
  UnitConversion                 Converts between compatible units
.claude/skills/
  save-recipe/  Claude skill that writes recipes to recipes.json
```

## Configuration

File locations are set by the `IngredientsFile` and `RecipesFile` settings.
They default to `ingredients.json` and `recipes.json` in the content root, and
the Docker image sets them to `/app/data/ingredients.json` and
`/app/data/recipes.json`.
