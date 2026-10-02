---
name: save-recipe
description: Save a recipe from the current conversation via the app's API so it shows up on the Saved Recipes page of the Blazor app. Use when the user says "save this recipe", "save that", "keep this recipe", or runs /save-recipe after Claude has given them a recipe.
---

# Save recipe

Saves a recipe through the app's API (`POST $API/recipes`, base URL in
`CLAUDE.md`). The app stores it in `recipes.json` on the server and shows it
at `/recipes`.

## Steps

1. **Find the recipe.** Use the most recent recipe Claude gave in this
   conversation, unless the user names a different one. If there is no recipe
   in the conversation, say so and stop — don't invent one.
2. **Build the entry** in the shape below. Leave out `id` — the server
   assigns it.
3. **POST it:**
   ```bash
   curl -s -X POST "$API/recipes" -H 'Content-Type: application/json' -d @- <<'JSON'
   { ...recipe... }
   JSON
   ```
   The response is the saved recipe with its `id`. A 400 means a field is
   invalid (usually a `unit`) — fix it and retry.
4. **Confirm** with one line: the title, the id, and that it's viewable at
   `/recipes/<id>` in the app.

Don't change ingredients. If the user also wants the used ingredients
deducted, that's a separate request (`POST $API/recipes/<id>/made`).

## Shape

```json
{
  "title": "Chickpea & Spinach Curry",
  "description": "A quick weeknight curry using tinned chickpeas.",
  "servings": 4,
  "prepMinutes": 10,
  "cookMinutes": 25,
  "ingredients": [
    { "name": "Chickpeas", "amount": 800, "unit": "g", "note": "drained" },
    { "name": "Onion", "amount": 1, "unit": "pcs", "note": "finely chopped" },
    { "name": "Salt", "note": "to taste" }
  ],
  "steps": [
    "Heat the oil in a large pan and soften the onion for 5 minutes.",
    "Add the chickpeas and simmer for 20 minutes."
  ],
  "tags": ["vegetarian", "curry"],
  "notes": "Keeps in the fridge for 3 days.",
  "createdAtUtc": "2026-09-29T12:00:00Z"
}
```

## Field rules

- `title` (required), `ingredients` and `steps` (required, non-empty).
- `unit` must be exactly one of: `g`, `kg`, `ml`, `l`, `tsp`, `tbsp`, `cup`,
  `pcs` — the API rejects anything else with a 400. Use `pcs` for counted items
  (1 onion, 2 cloves garlic). If an ingredient has no sensible measure
  ("to taste", "a pinch"), leave out `amount` and `unit` and put it in `note`.
  Convert anything else to one of the allowed units (e.g. a "handful" of
  spinach → estimate grams).
- `amount` is a number, never a string or fraction (`0.5`, not `"1/2"`).
- `steps` are plain strings without leading numbers — the app numbers them.
- `servings`, `prepMinutes`, `cookMinutes` are integers; leave a field out
  rather than guessing wildly.
- `tags` are short lowercase words (dish type, cuisine, "vegetarian" etc.).
- Omit `description` and `notes` if there's nothing useful to say.
- `createdAtUtc` is the current UTC time in ISO 8601 with a `Z` suffix.
- Leave out `isFavourite` and `lastUsedUtc` on new recipes — the app sets them
  (favourite button, "I made this" button).
