# Recipe Ingredients

A Blazor Server app for tracking the ingredients you have on hand. Data is
stored in PostgreSQL, and a small JSON API under `/api` lets Claude read and
update it from another machine.

## What it does

- **Ingredients** — add, edit, and delete the ingredients you have available,
  with an amount, unit, quantity on hand, and whether each is frozen or canned.
- **Kitchen Staples** — a separate page (`/staples`) for spices, oils, sauces
  and other basics. No amounts, just a switch for whether each one is on hand.
  Claude uses the available ones when generating recipes; they're never
  deducted.
- **Recipes** — ask Claude (e.g. in Claude Code, from this folder) for a recipe
  using the ingredients in stock. See [CLAUDE.md](CLAUDE.md).
- **Saved Recipes** — run `/save-recipe` in Claude Code to save a recipe;
  saved recipes are listed at `/recipes` with servings, total time, tags and
  nutrition (kcal, protein, carbs per serving). You can view, favourite
  (favourites sort first), or delete them.
- **"I made this"** — on a recipe's page, deducts its ingredients from stock
  and records when you last made it. Names are matched case-insensitively
  (ignoring plurals) and units are converted within the same kind
  (g/kg, ml/l/tsp/tbsp/cup, pcs).
- **Settings** — export all ingredients, staples and recipes as one JSON file,
  or import a previous export (replaces current data).

> [!NOTE]
> Partly used items keep their pack size: 2 × 400 g minus 200 g leaves
> 1 × 400 g plus a separate 1 × 200 g entry. Canned items are deducted by
> count rather than weight. Recipe ingredients with no quantity, no matching
> stock, or incompatible units are skipped and listed in the result.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A PostgreSQL database

## Configuration

The app needs a connection string in `ConnectionStrings:Default`; it won't
start without one.

| Source | How |
|---|---|
| `appsettings.Development.json` / user secrets / env var | `ConnectionStrings__Default="Host=...;Database=...;Username=...;Password=..."` |
| Docker secret | A file named `ConnectionStrings__Default` in `/run/secrets/` |

On startup the app creates the tables if they don't exist. If the database is
empty it seeds itself from `ingredients.json`, `staples.json` and
`recipes.json` in the content root (override the paths with the
`IngredientsFile`, `StaplesFile` and `RecipesFile` settings), when present.

## Running locally

```bash
dotnet restore
dotnet run
```

The app starts at `http://localhost:5122` (see
[Properties/launchSettings.json](Properties/launchSettings.json) for the
HTTPS profile).

## Running with Docker

Put the connection string in `secrets/db_connection_string.txt` (git-ignored),
then:

```bash
docker compose up -d --build
```

The container listens on port 8080 inside, mapped to **8090** on the host by
[docker-compose.yml](docker-compose.yml). The connection string is supplied
as a Docker secret.

## Tests

```bash
dotnet test RecipeIngredients.Tests
```

Covers unit conversion and ingredient usage/deduction.

## API

Unauthenticated JSON endpoints for Claude (or anything else on the LAN):

| Method | Path | Does |
|---|---|---|
| GET | `/api/ingredients` | List ingredients |
| POST | `/api/ingredients` | Add an ingredient |
| PUT | `/api/ingredients/{id}` | Update an ingredient |
| DELETE | `/api/ingredients/{id}` | Delete an ingredient |
| POST | `/api/ingredients/use` | Deduct `[{name, amount, unit}]` from stock |
| GET | `/api/staples` | List kitchen staples |
| POST | `/api/staples` | Add a staple `{name, available}` |
| PUT | `/api/staples/{id}` | Update a staple |
| DELETE | `/api/staples/{id}` | Delete a staple |
| GET | `/api/recipes` | List saved recipes |
| GET | `/api/recipes/{id}` | Get one recipe |
| POST | `/api/recipes` | Save a recipe (id assigned by the server) |
| DELETE | `/api/recipes/{id}` | Delete a recipe |
| POST | `/api/recipes/{id}/made` | Same as "I made this" |

```bash
curl http://SERVER_HOST:8090/api/ingredients
```

Set the server address in [CLAUDE.md](CLAUDE.md) so Claude Code uses it.

## Tech stack

- ASP.NET Core Blazor Server (.NET 10, interactive server render mode)
- [MudBlazor](https://mudblazor.com/) for UI components
- Entity Framework Core with PostgreSQL (Npgsql)

## Project structure

```
Components/
  Layout/       Main layout and nav menu
  Pages/        Ingredients, Staples, Recipes, RecipeDetail, Settings, Error, NotFound
  Dialogs/      Add/edit ingredient dialog
Data/           AppDbContext (EF Core)
Endpoints/      Ingredient, Staple and Recipe endpoints (JSON API under /api)
Models/         Ingredient, Staple, Recipe, RecipeIngredient, Nutrition, MeasurementUnit
Services/
  Db*Store                       EF Core persistence (behind I*Store interfaces)
  IngredientUsage                Deducts a recipe's ingredients from stock
  UnitConversion                 Converts between compatible units
RecipeIngredients.Tests/        Unit tests
.claude/skills/
  pantry-recipe/ Claude skill that writes recipes from stock via the API
  save-recipe/   Claude skill that saves recipes via POST /api/recipes
```
