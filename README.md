# Recipe Ingredients

A JSON API for tracking the ingredients you have on hand. Data is stored in
PostgreSQL. Every endpoint (including `/health`) requires the `X-Api-Key` header.

## What it does

- **Ingredients** — amount, unit, quantity on hand, frozen/canned flags.
- **Kitchen Staples** — spices, oils, sauces; just whether each is on hand.
- **Saved Recipes** — servings, time, tags, nutrition, favourites.
- **"I made this"** — deducts a recipe's ingredients from stock. Names are
  matched case-insensitively (ignoring plurals) and units are converted within
  the same kind (g/kg, ml/l/tsp/tbsp/cup, pcs).

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

JSON endpoints, all requiring the `X-Api-Key` header (`Api:Key` / `Api__Key` secret):

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
curl -H "X-Api-Key: $KEY" http://SERVER_HOST:8090/api/ingredients
```

Set the server address in [CLAUDE.md](CLAUDE.md) so Claude Code uses it.

## Tech stack

- ASP.NET Core minimal APIs (.NET 10)
- Entity Framework Core with PostgreSQL (Npgsql)

## Project structure

```
Data/           AppDbContext (EF Core)
Endpoints/      Ingredient, Staple and Recipe endpoints, API key filter
Models/         Ingredient, Staple, Recipe, RecipeIngredient, Nutrition, MeasurementUnit
Services/       Db*Store persistence, IngredientUsage, UnitConversion
RecipeIngredients.Tests/        Unit tests
```
