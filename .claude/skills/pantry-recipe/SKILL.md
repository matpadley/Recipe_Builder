---
name: pantry-recipe
description: Generate a vegetarian, metric recipe in this project from the ingredients actually on hand, read from the Recipe Ingredients app's API. Use whenever the user in this repo asks for a recipe, what to cook or make, what they can make with what they have, or names a dish or cuisine — including when they mention meat or fish, which gets swapped for a vegetarian substitute. Also triggered when the user specifies a meal type: breakfast, lunch, dinner, or dessert. Use this instead of the global recipe-generator skill in this project.
---

# Pantry recipe

Turn the user's request plus the ingredients on hand into one complete,
cookable vegetarian recipe. All data comes from the app's API, never from
local files.

## API

The API SERVER_HOST is http://docke.local:8090

Base URL is the `API=` line in `CLAUDE.md` (e.g. `http://SERVER_HOST:8090/api`).
Call it with `curl -s`; send JSON with `-H 'Content-Type: application/json'`.

| Method | Path | Body | Use it to |
|---|---|---|---|
| GET | `/ingredients` | | Get the stock list (always, first) |
| GET | `/staples` | | Get the kitchen staples (always) |
| GET | `/recipes` | | See saved recipes — avoid repeats, find favourites |
| GET | `/recipes/{id}` | | Fetch one saved recipe |
| POST | `/recipes` | recipe | Save a recipe — via the `save-recipe` skill |
| POST | `/recipes/{id}/made` | | Deduct a saved recipe's ingredients and mark it used |
| POST | `/ingredients/use` | `[{name, amount, unit}]` | Deduct an unsaved recipe's ingredients |
| POST | `/ingredients` | ingredient | Add stock (only when asked) |
| PUT | `/ingredients/{id}` | ingredient | Change stock (only when asked) |
| DELETE | `/ingredients/{id}` | | Remove stock (only when asked) |
| POST | `/staples` | `{name, available}` | Add a staple (only when asked) |
| PUT | `/staples/{id}` | `{name, available}` | Mark a staple run out/restocked (only when asked) |
| DELETE | `/staples/{id}` | | Remove a staple (only when asked) |

Stock entries look like:

```json
{ "id": 1, "name": "Chickpeas", "amount": 400, "unit": "g", "available": 2, "frozen": false }
```

`amount` + `unit` is one item; `available` is how many items, so the total on
hand is `amount × available` (here 800 g). `frozen: true` means it's in the
freezer, so mention defrosting it in the recipe; a missing `frozen` means false.

Staples (spices, oils, sauces, etc.) have no amounts — only whether they're
on hand:

```json
{ "id": 1, "name": "Ground cumin", "available": true }
```

If the API can't be reached, say so and stop. Don't fall back to a local
`ingredients.json` — it's out of date.

## Steps

1. **Get stock and staples:** `GET /ingredients` and `GET /staples`.
2. **Get saved recipes:** `GET /recipes`. Don't suggest something that's
   already saved unless the user asks for it. Prefer something different from
   recently made recipes (`lastUsedUtc`). If the user asks for "a favourite" or
   "something I've made before", pick from these (`isFavourite`) and check it
   against the current stock.
3. **Pick the dish.** Use the user's dish or cuisine if they named one;
   otherwise pick one that uses the most of what's on hand. Build it around the
   stock, and never use more of an item than its total on hand. If something
   essential is missing, say so in Notes and keep it minor (or swap it).
   If the user specifies a meal type (breakfast, lunch, dinner, dessert), apply
   the guidance in the **Meal types** section below. Ask a question only if
   nothing sensible can be made.
4. **Write the recipe** in the format below.
5. **Save the recipe automatically** via `POST /recipes` — do this immediately
   after writing it, without waiting to be asked. Use the shape in the **Saving**
   section below. Confirm with one line: the title, the id, and that it's
   viewable at `/recipes/<id>` in the app.
6. **Offer one next step** once it's cooked: deduct the ingredients with
   `POST /recipes/{id}/made`. Don't do this unless the user asks.

## Always vegetarian

No meat, poultry or fish; eggs and dairy are fine. If the user or the stock
mentions meat or fish, swap it for a substitute that fits the dish, e.g.:

- Chicken → chickpeas, tofu, paneer, king oyster mushrooms
- Beef/lamb → mushrooms, lentils, plant-based mince
- Fish/prawns → tofu, tempeh, hearts of palm

Prefer a substitute that's actually in stock. Mention the swap in one short
sentence in Notes.

## Kitchen staples

Staples with `available: true` are on hand in whatever amount the recipe
needs — use them freely to season and flavour the dish, and list them only if
the recipe uses them. Give them normal recipe quantities (`1 tsp Ground
cumin`) using the staple's name. They aren't stock, so deduction skips them;
don't add them to the **Uses from stock** line.

Staples with `available: false` have run out: don't use them. If one would
really suit the dish, swap it for an available staple or mention it in Notes.

If something is both a staple and in stock, treat it as stock (it gets
deducted).

Beyond the staples list, assume only salt, black pepper and water. If the
staples list is empty (not set up yet), fall back to assuming: onion, garlic,
salt, black pepper, cooking oil/butter, paprika, cumin, chilli flakes,
oregano, bay leaf.

## Quantities

Ingredient lines must work unchanged with `save-recipe` and the deduction
endpoints:

- Units only: `g`, `kg`, `ml`, `l`, `tsp`, `tbsp`, `cup`, `pcs`. Use metric
  weights and volumes; use `pcs` for counted items (2 pcs garlic cloves,
  1 pcs onion). No ounces, pounds or °F.
- Amounts are decimals, never fractions: `0.5 tsp`, not `1/2 tsp`.
- Use the **same unit kind as the stock entry** (stock in `g` → recipe in `g`
  or `kg`; stock in `pcs` → `pcs`), so deduction can match it.
- Use the **stock item's name** for the ingredient (e.g. "Chickpeas" if that's
  the stock name), so deduction finds it. Preparation goes after a comma
  ("Chickpeas, drained").
- Things with no real measure ("salt, to taste") get no amount.
- Temperatures in °C.

Default to 2 servings unless the user says otherwise; scale quantities to
match.

## Meal types

If the user specifies a meal type, constrain the dish and tag accordingly.

**breakfast** — lighter, quicker dishes suited to the morning. Lean towards
eggs, porridge, pancakes, fritters, or grain bowls. Prefer shorter cook times
(under 20 min). Avoid heavy sauces and spicy profiles. Tag: `"breakfast"`.

**lunch** — satisfying but not heavy. Soups, noodle broths, bean salads, orzo
dishes, or light pasta. A moderate amount of stock items. Tag: `"lunch"`.

**dinner** — the default when no type is given. Full mains: curries, stews,
fried rice, pasta bakes, noodle dishes. Use the most of what's on hand.
Tag: `"dinner"`.

**dessert** — sweet dishes only. Draw on coconut milk, creamed coconut,
ground almonds, flaked almonds, cashew nuts, flour, rice, or tofu (silken
tofu works well in mousse/pudding). Don't use savoury sauces or stock.
Tag: `"dessert"`.

Always add the meal type as a tag in the saved recipe alongside any other
relevant tags (cuisine, main ingredient, etc.).

## Output format

Reply in chat:

```
## [Recipe Name]
Serves 2 · 10 min prep · 25 min cook

**Ingredients**
- 400 g Chickpeas, drained
- 200 ml Coconut milk
- 1 pcs Onion, finely diced
- 2 pcs Garlic cloves, minced
- 1 tbsp Cooking oil
- 1 tsp Paprika
- 0.5 tsp Chilli flakes (optional)
- Salt, to taste

**Method**
1. Heat the oil in a pan over a medium heat. Add the onion and cook for 5 minutes until soft.
2. ...

**Uses from stock:** Chickpeas 400 of 800 g · Coconut milk 200 of 400 ml

**Notes** (optional, 1–2 lines: swaps, missing items, storage, serving)
```

The **Uses from stock** line shows how much of each stock item the recipe
takes out of the total on hand, so the user can see what's left.

## Method style

Concrete steps in the order you'd actually cook them, one action per step,
with real times and heat levels ("simmer for 12 minutes on a low heat"), never
"cook until done".

## Saving

After displaying the recipe, POST it immediately:

```bash
curl -s -X POST "http://docke.local:8090/api/recipes" \
  -H 'Content-Type: application/json' -d @- <<'JSON'
{ ...recipe... }
JSON
```

Shape:

```json
{
  "title": "Recipe Name",
  "description": "One sentence summary.",
  "servings": 2,
  "prepMinutes": 10,
  "cookMinutes": 25,
  "ingredients": [
    { "name": "Chickpeas", "amount": 400, "unit": "g", "note": "drained" },
    { "name": "Salt", "note": "to taste" }
  ],
  "nutrition": { "kcal": 520, "proteinG": 22, "carbsG": 68 },
  "steps": [
    "Heat the oil in a pan over a medium heat.",
    "Add the onion and cook for 5 minutes until soft."
  ],
  "tags": ["vegetarian"],
  "notes": "Optional 1–2 lines.",
  "createdAtUtc": "2026-10-07T12:00:00Z"
}
```

Field rules:
- `unit` must be exactly one of: `g`, `kg`, `ml`, `l`, `tsp`, `tbsp`, `cup`,
  `pcs`. Anything else gives a 400 — fix and retry.
- Ingredients with no real measure (salt, pepper) omit `amount` and `unit`;
  put it in `note` instead.
- `nutrition` is required: estimated values **per serving** — `kcal` (integer),
  `proteinG` and `carbsG` (grams). Work them out from the final ingredient list
  (typical values per 100 g, summed, divided by `servings`).
- `steps` are plain strings with no leading numbers.
- `createdAtUtc` is the current UTC time in ISO 8601 with a `Z` suffix.
- Leave out `isFavourite` and `lastUsedUtc` — the app sets them.
