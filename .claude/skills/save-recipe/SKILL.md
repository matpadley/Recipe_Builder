---
name: save-recipe
description: Save a recipe from the current conversation to recipes.json so it shows up on the Saved Recipes page of the Blazor app. Use when the user says "save this recipe", "save that", "keep this recipe", or runs /save-recipe after Claude has given them a recipe.
---

# Save recipe

Saves a recipe to `recipes.json` in the project root (next to `ingredients.json`).
The Blazor app reads this file and shows it at `/recipes`.

## Steps

1. **Find the recipe.** Use the most recent recipe Claude gave in this
   conversation, unless the user names a different one. If there is no recipe
   in the conversation, say so and stop — don't invent one.
2. **Read `recipes.json`.** If it doesn't exist, treat it as `[]`.
3. **Pick the id.** `id` = highest existing `id` + 1, or `1` if the file is empty.
4. **Build the entry** in the shape below and append it to the array.
5. **Write the file** back as an indented JSON array (2 spaces), keeping every
   existing entry unchanged.
6. **Confirm** with one line: the title, the id, and that it's viewable at
   `/recipes/<id>` in the app.

Don't touch `ingredients.json`. If the user also wants the used ingredients
deducted, that's a separate request.

## Shape

```json
{
  "id": 3,
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
  `pcs` — the app fails to load the file otherwise. Use `pcs` for counted items
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
