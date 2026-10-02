using RecipeIngredients.Models;

namespace RecipeIngredients.Services;

/// <summary>
/// Converts between units of the same kind: mass (g, kg), volume (ml, l, tsp,
/// tbsp, cup) and counts (pcs). Mass and volume can't be converted to each other.
/// </summary>
public static class UnitConversion
{
    private enum Kind { Mass, Volume, Count }

    private static (Kind Kind, decimal Factor) BaseOf(MeasurementUnit unit) => unit switch
    {
        MeasurementUnit.g => (Kind.Mass, 1m),
        MeasurementUnit.kg => (Kind.Mass, 1000m),
        MeasurementUnit.ml => (Kind.Volume, 1m),
        MeasurementUnit.l => (Kind.Volume, 1000m),
        MeasurementUnit.tsp => (Kind.Volume, 5m),
        MeasurementUnit.tbsp => (Kind.Volume, 15m),
        MeasurementUnit.cup => (Kind.Volume, 240m),
        MeasurementUnit.pcs => (Kind.Count, 1m),
        _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null),
    };

    public static bool TryConvert(decimal amount, MeasurementUnit from, MeasurementUnit to, out decimal result)
    {
        var source = BaseOf(from);
        var target = BaseOf(to);
        if (source.Kind != target.Kind)
        {
            result = 0;
            return false;
        }

        result = amount * source.Factor / target.Factor;
        return true;
    }
}
