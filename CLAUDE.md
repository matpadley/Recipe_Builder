# Recipe Ingredients

The ingredients the user has on hand are stored in `ingredients.json` in this
folder (maintained via the Blazor app). When asked for a recipe, read that file
and base the recipe on those ingredients.

Each entry looks like:

```json
{ "id": 1, "name": "Chickpeas", "amount": 400, "unit": "g", "available": 2, "createdAtUtc": "..." }
```

- `amount` + `unit` is the size of one item (units: g, kg, ml, l, tsp, tbsp, cup, pcs).
- `available` is how many of those items are on hand, so total = amount × available.

Don't edit `ingredients.json` unless asked (e.g. to deduct what a recipe used).

# Saved Recipes

Recipes the user chooses to keep are stored in `recipes.json` in this folder
and shown on the app's Saved Recipes page (`/recipes`). Save them with the
`save-recipe` skill (`.claude/skills/save-recipe/SKILL.md`), which defines the
JSON shape. Only save a recipe when the user asks.
