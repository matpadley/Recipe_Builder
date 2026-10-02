using RecipeIngredients.Models;

namespace RecipeIngredients.Services;

public enum UsageOutcome
{
    Deducted,
    RanOut,
    Skipped,
}

public record UsageLine(string Name, UsageOutcome Outcome, string Detail);

/// <summary>
/// Deducts a recipe's ingredients from the stock list. Matches by name
/// (case-insensitive, ignoring a plural "s"/"es") and converts units where the
/// kinds are compatible. Partially used items keep their original pack size:
/// e.g. 2 × 400 g minus 200 g leaves 1 × 400 g plus a separate 1 × 200 g entry.
/// </summary>
public static class IngredientUsage
{
    public static List<UsageLine> Apply(List<Ingredient> stock, IEnumerable<RecipeIngredient> used)
    {
        var lines = new List<UsageLine>();

        foreach (var item in used)
        {
            if (item.Amount is not { } amount || item.Unit is not { } unit || amount <= 0)
            {
                lines.Add(new(item.Name, UsageOutcome.Skipped, "no quantity in recipe"));
                continue;
            }

            var matches = stock.Where(s => NamesMatch(s.Name, item.Name)).ToList();
            if (matches.Count == 0)
            {
                lines.Add(new(item.Name, UsageOutcome.Skipped, "not in your ingredients"));
                continue;
            }

            // Use up the smallest (usually partly used) entries first.
            var compatible = matches
                .Where(s => UnitConversion.TryConvert(1, unit, s.Unit, out _))
                .OrderBy(s => Total(s))
                .ToList();
            if (compatible.Count == 0)
            {
                lines.Add(new(item.Name, UsageOutcome.Skipped,
                    $"recipe uses {unit}, stock is in {matches[0].Unit}"));
                continue;
            }

            var needed = amount;
            foreach (var entry in compatible)
            {
                UnitConversion.TryConvert(needed, unit, entry.Unit, out var neededInEntryUnit);
                var total = Total(entry);
                var take = Math.Min(total, neededInEntryUnit);
                SetRemaining(stock, entry, total - take);

                UnitConversion.TryConvert(take, entry.Unit, unit, out var takenInRecipeUnit);
                needed -= takenInRecipeUnit;
                if (needed <= 0.005m)
                {
                    break;
                }
            }

            var usedText = $"{amount:0.##} {unit}";
            lines.Add(needed > 0.005m
                ? new(item.Name, UsageOutcome.RanOut, $"used {usedText}, but you were {needed:0.##} {unit} short — now used up")
                : new(item.Name, UsageOutcome.Deducted, $"−{usedText}"));
        }

        return lines;
    }

    private static decimal Total(Ingredient ingredient) => ingredient.Amount * ingredient.Available;

    private static void SetRemaining(List<Ingredient> stock, Ingredient entry, decimal remaining)
    {
        remaining = Math.Round(remaining, 2);
        if (remaining < 0.01m)
        {
            stock.Remove(entry);
            return;
        }

        var fullItems = (int)Math.Floor(remaining / entry.Amount);
        var partial = Math.Round(remaining - fullItems * entry.Amount, 2);

        if (fullItems == 0)
        {
            entry.Amount = partial;
            entry.Available = 1;
            return;
        }

        entry.Available = fullItems;
        if (partial >= 0.01m)
        {
            stock.Add(new Ingredient
            {
                Id = stock.Max(s => s.Id) + 1,
                Name = entry.Name,
                Amount = partial,
                Unit = entry.Unit,
                Available = 1,
                CreatedAtUtc = entry.CreatedAtUtc,
            });
        }
    }

    private static bool NamesMatch(string a, string b)
    {
        a = a.Trim().ToLowerInvariant();
        b = b.Trim().ToLowerInvariant();
        return a == b
            || a + "s" == b || b + "s" == a
            || a + "es" == b || b + "es" == a;
    }
}
