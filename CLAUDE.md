# Recipe Ingredients

The app runs on a separate server, so the data is reached over its HTTP API,
not local files. Use `curl` against this base URL:

```
API=http://SERVER_HOST:8090/api
```

| Method | Path | Body | Does |
|---|---|---|---|
| GET | `/ingredients` | | List ingredients on hand |
| POST | `/ingredients` | ingredient | Add an ingredient (id assigned) |
| PUT | `/ingredients/{id}` | ingredient | Update name/amount/unit/available |
| DELETE | `/ingredients/{id}` | | Remove an ingredient |
| POST | `/ingredients/use` | `[{name, amount, unit}]` | Deduct amounts from stock |
| GET | `/recipes` | | List saved recipes |
| GET | `/recipes/{id}` | | Get one recipe |
| POST | `/recipes` | recipe | Save a recipe (id assigned) |
| DELETE | `/recipes/{id}` | | Delete a recipe |
| POST | `/recipes/{id}/made` | | Deduct its ingredients and mark it used |

Send JSON with `-H 'Content-Type: application/json'`. Invalid units give a 400.

## Ingredients

When asked for a recipe, use the `pantry-recipe` skill
(`.claude/skills/pantry-recipe/SKILL.md`), which reads `GET $API/ingredients`
and bases the recipe on those ingredients. Each entry looks like:

```json
{ "id": 1, "name": "Chickpeas", "amount": 400, "unit": "g", "available": 2, "frozen": false, "createdAtUtc": "..." }
```

- `amount` + `unit` is the size of one item (units: g, kg, ml, l, tsp, tbsp, cup, pcs).
- `available` is how many of those items are on hand, so total = amount × available.
- `frozen` is true if the item is in the freezer (needs defrosting). It's
  optional and defaults to false, so entries without it are not frozen.

Don't change ingredients unless asked (e.g. to deduct what a recipe used —
use `/ingredients/use` or `/recipes/{id}/made`).

# Saved Recipes

Recipes the user chooses to keep are saved through `POST $API/recipes` and
shown on the app's Saved Recipes page (`/recipes`). Save them with the
`save-recipe` skill (`.claude/skills/save-recipe/SKILL.md`), which defines the
JSON shape. Only save a recipe when the user asks.
