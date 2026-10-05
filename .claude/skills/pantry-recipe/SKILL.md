---
name: pantry-recipe
description: Generate a vegetarian, metric recipe in this project from the ingredients actually on hand, read from the Recipe Ingredients app's API. Use whenever the user in this repo asks for a recipe, what to cook or make, what they can make with what they have, or names a dish or cuisine — including when they mention meat or fish, which gets swapped for a vegetarian substitute. Use this instead of the global recipe-generator skill in this project.
---

# Pantry recipe

Turn the user's request plus the ingredients on hand into one complete,
cookable vegetarian recipe. All data comes from the app's API, never from
local files.

## API

Base URL is the `API=` line in `CLAUDE.md` (e.g. `http://SERVER_HOST:8090/api`).
Call it with `curl -s`; send JSON with `-H 'Content-Type: application/json'`.

| Method | Path | Body | Use it to |
|---|---|---|---|
| GET | `/ingredients` | | Get the stock list (always, first) |
| GET | `/recipes` | | See saved recipes — avoid repeats, find favourites |
| GET | `/recipes/{id}` | | Fetch one saved recipe |
| POST | `/recipes` | recipe | Save a recipe — via the `save-recipe` skill |
| POST | `/recipes/{id}/made` | | Deduct a saved recipe's ingredients and mark it used |
| POST | `/ingredients/use` | `[{name, amount, unit}]` | Deduct an unsaved recipe's ingredients |
| POST | `/ingredients` | ingredient | Add stock (only when asked) |
| PUT | `/ingredients/{id}` | ingredient | Change stock (only when asked) |
| DELETE | `/ingredients/{id}` | | Remove stock (only when asked) |

Stock entries look like:

```json
{ "id": 1, "name": "Chickpeas", "amount": 400, "unit": "g", "available": 2 }
```

`amount` + `unit` is one item; `available` is how many items, so the total on
hand is `amount × available` (here 800 g).

If the API can't be reached, say so and stop. Don't fall back to a local
`ingredients.json` — it's out of date.

## Steps

1. **Get stock:** `GET /ingredients`.
2. **Get saved recipes:** `GET /recipes`. Don't suggest something that's
   already saved unless the user asks for it. Prefer something different from
   recently made recipes (`lastUsedUtc`). If the user asks for "a favourite" or
   "something I've made before", pick from these (`isFavourite`) and check it
   against the current stock.
3. **Pick the dish.** Use the user's dish or cuisine if they named one;
   otherwise pick one that uses the most of what's on hand. Build it around the
   stock, and never use more of an item than its total on hand. If something
   essential is missing, say so in Notes and keep it minor (or swap it).
   Ask a question only if nothing sensible can be made.
4. **Write the recipe** in the format below.
5. **Offer next steps** in one line: save it (`/save-recipe`), and once it's
   cooked, deduct the ingredients — `POST /recipes/{id}/made` if it's saved,
   otherwise `POST /ingredients/use`. Do neither unless the user asks.

## Always vegetarian

No meat, poultry or fish; eggs and dairy are fine. If the user or the stock
mentions meat or fish, swap it for a substitute that fits the dish, e.g.:

- Chicken → chickpeas, tofu, paneer, king oyster mushrooms
- Beef/lamb → mushrooms, lentils, plant-based mince
- Fish/prawns → tofu, tempeh, hearts of palm

Prefer a substitute that's actually in stock. Mention the swap in one short
sentence in Notes.

## Pantry assumptions

Assume these are always in the kitchen even if they're not in stock: onion,
garlic, salt, black pepper, cooking oil/butter, paprika, cumin, chilli flakes,
oregano, bay leaf. List them only if the recipe uses them. If one is also in
stock, treat it as stock (it gets deducted).

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
