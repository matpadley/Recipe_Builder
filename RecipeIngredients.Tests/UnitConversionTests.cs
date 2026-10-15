using RecipeIngredients.Models;
using RecipeIngredients.Services;

namespace RecipeIngredients.Tests;

public class UnitConversionTests
{
    [TestCase(1, MeasurementUnit.kg, MeasurementUnit.g, 1000)]
    [TestCase(500, MeasurementUnit.g, MeasurementUnit.kg, 0.5)]
    [TestCase(1, MeasurementUnit.l, MeasurementUnit.ml, 1000)]
    [TestCase(1, MeasurementUnit.tbsp, MeasurementUnit.tsp, 3)]
    [TestCase(1, MeasurementUnit.cup, MeasurementUnit.ml, 240)]
    [TestCase(3, MeasurementUnit.pcs, MeasurementUnit.pcs, 3)]
    public void Converts_within_a_kind(double amount, MeasurementUnit from, MeasurementUnit to, double expected)
    {
        Assert.That(UnitConversion.TryConvert((decimal)amount, from, to, out var result), Is.True);
        Assert.That(result, Is.EqualTo((decimal)expected));
    }

    [TestCase(MeasurementUnit.g, MeasurementUnit.ml)]
    [TestCase(MeasurementUnit.ml, MeasurementUnit.pcs)]
    [TestCase(MeasurementUnit.pcs, MeasurementUnit.kg)]
    public void Refuses_across_kinds(MeasurementUnit from, MeasurementUnit to)
    {
        Assert.That(UnitConversion.TryConvert(1, from, to, out var result), Is.False);
        Assert.That(result, Is.Zero);
    }
}
