using RecipeIngredients.Models;
using RecipeIngredients.Services;

namespace RecipeIngredients.Tests;

public class IngredientUsageTests
{
    private static Ingredient Stock(int id, string name, decimal amount, MeasurementUnit unit, int available = 1,
        bool canned = false, bool frozen = false) => new()
    {
        Id = id, Name = name, Amount = amount, Unit = unit, Available = available, Canned = canned, Frozen = frozen,
    };

    private static RecipeIngredient Need(string name, decimal? amount, MeasurementUnit? unit) =>
        new() { Name = name, Amount = amount, Unit = unit };

    private static UsageLine Apply(List<Ingredient> stock, params RecipeIngredient[] used) =>
        IngredientUsage.Apply(stock, used).Single();

    [Test]
    public void Exact_deduction_removes_the_entry()
    {
        var stock = new List<Ingredient> { Stock(1, "Flour", 500, MeasurementUnit.g) };

        var line = Apply(stock, Need("Flour", 500, MeasurementUnit.g));

        Assert.That(line.Outcome, Is.EqualTo(UsageOutcome.Deducted));
        Assert.That(stock, Is.Empty);
    }

    [Test]
    public void Names_match_case_insensitively_and_ignoring_plural()
    {
        var stock = new List<Ingredient> { Stock(1, "Tomatoes", 4, MeasurementUnit.pcs) };

        var line = Apply(stock, Need("tomato", 1, MeasurementUnit.pcs));

        Assert.That(line.Outcome, Is.EqualTo(UsageOutcome.Deducted));
        Assert.That(stock.Single().Amount, Is.EqualTo(3));
    }

    [Test]
    public void Partial_pack_is_split_and_keeps_flags()
    {
        var stock = new List<Ingredient> { Stock(1, "Rice", 400, MeasurementUnit.g, available: 2, frozen: true) };

        Apply(stock, Need("Rice", 200, MeasurementUnit.g));

        Assert.That(stock, Has.Count.EqualTo(2));
        var full = stock.Single(s => s.Id == 1);
        var partial = stock.Single(s => s.Id != 1);
        Assert.That((full.Amount, full.Available), Is.EqualTo((400m, 1)));
        Assert.That((partial.Amount, partial.Available), Is.EqualTo((200m, 1)));
        Assert.That(partial.Id, Is.EqualTo(2));
        Assert.That(partial.Frozen, Is.True);
    }

    [Test]
    public void Smallest_entry_is_used_first()
    {
        var stock = new List<Ingredient>
        {
            Stock(1, "Oats", 500, MeasurementUnit.g),
            Stock(2, "Oats", 100, MeasurementUnit.g),
        };

        Apply(stock, Need("Oats", 150, MeasurementUnit.g));

        Assert.That(stock.Select(s => s.Id), Is.EqualTo(new[] { 1 }));
        Assert.That(stock.Single().Amount, Is.EqualTo(450));
    }

    [Test]
    public void Converts_between_units_of_the_same_kind()
    {
        var stock = new List<Ingredient> { Stock(1, "Milk", 1, MeasurementUnit.l) };

        var line = Apply(stock, Need("Milk", 250, MeasurementUnit.ml));

        Assert.That(line.Outcome, Is.EqualTo(UsageOutcome.Deducted));
        Assert.That(stock.Single().Amount, Is.EqualTo(0.75m));
    }

    [Test]
    public void Incompatible_units_are_skipped_and_stock_untouched()
    {
        var stock = new List<Ingredient> { Stock(1, "Milk", 1, MeasurementUnit.l) };

        var line = Apply(stock, Need("Milk", 100, MeasurementUnit.g));

        Assert.That(line.Outcome, Is.EqualTo(UsageOutcome.Skipped));
        Assert.That(stock.Single().Amount, Is.EqualTo(1));
    }

    [Test]
    public void Running_short_reports_shortfall_and_uses_everything()
    {
        var stock = new List<Ingredient> { Stock(1, "Butter", 100, MeasurementUnit.g) };

        var line = Apply(stock, Need("Butter", 130, MeasurementUnit.g));

        Assert.That(line.Outcome, Is.EqualTo(UsageOutcome.RanOut));
        Assert.That(line.Detail, Does.Contain("30"));
        Assert.That(stock, Is.Empty);
    }

    [Test]
    public void Need_spanning_several_entries_drains_them_in_turn()
    {
        var stock = new List<Ingredient>
        {
            Stock(1, "Sugar", 100, MeasurementUnit.g),
            Stock(2, "Sugar", 200, MeasurementUnit.g),
        };

        var line = Apply(stock, Need("Sugar", 250, MeasurementUnit.g));

        Assert.That(line.Outcome, Is.EqualTo(UsageOutcome.Deducted));
        Assert.That(stock.Single().Amount, Is.EqualTo(50));
    }

    [Test]
    public void Line_without_quantity_is_skipped()
    {
        var stock = new List<Ingredient> { Stock(1, "Salt", 500, MeasurementUnit.g) };

        var line = Apply(stock, Need("Salt", null, null));

        Assert.That(line.Outcome, Is.EqualTo(UsageOutcome.Skipped));
        Assert.That(stock.Single().Amount, Is.EqualTo(500));
    }

    [Test]
    public void Unknown_ingredient_is_skipped()
    {
        var stock = new List<Ingredient> { Stock(1, "Salt", 500, MeasurementUnit.g) };

        var line = Apply(stock, Need("Saffron", 1, MeasurementUnit.g));

        Assert.That(line.Outcome, Is.EqualTo(UsageOutcome.Skipped));
    }

    // --- Canned: deductions count cans, not weight ---

    [Test]
    public void Canned_opens_a_whole_can_and_discards_the_rest()
    {
        var stock = new List<Ingredient> { Stock(1, "Chickpeas", 400, MeasurementUnit.g, available: 2, canned: true) };

        var line = Apply(stock, Need("Chickpeas", 150, MeasurementUnit.g));

        Assert.That(line.Outcome, Is.EqualTo(UsageOutcome.Deducted));
        Assert.That(stock, Has.Count.EqualTo(1));
        Assert.That((stock[0].Amount, stock[0].Available), Is.EqualTo((400m, 1)));
    }

    [Test]
    public void Canned_opens_as_many_cans_as_needed()
    {
        var stock = new List<Ingredient> { Stock(1, "Chickpeas", 400, MeasurementUnit.g, available: 3, canned: true) };

        Apply(stock, Need("Chickpeas", 500, MeasurementUnit.g));

        Assert.That(stock.Single().Available, Is.EqualTo(1));
    }

    [Test]
    public void Canned_removes_entry_when_last_can_is_used()
    {
        var stock = new List<Ingredient> { Stock(1, "Chickpeas", 400, MeasurementUnit.g, canned: true) };

        Apply(stock, Need("Chickpeas", 400, MeasurementUnit.g));

        Assert.That(stock, Is.Empty);
    }

    [Test]
    public void Canned_runs_out_when_not_enough_cans()
    {
        var stock = new List<Ingredient> { Stock(1, "Chickpeas", 400, MeasurementUnit.g, available: 1, canned: true) };

        var line = Apply(stock, Need("Chickpeas", 600, MeasurementUnit.g));

        Assert.That(line.Outcome, Is.EqualTo(UsageOutcome.RanOut));
        Assert.That(stock, Is.Empty);
    }

    [Test]
    public void Canned_converts_recipe_units_to_can_units()
    {
        var stock = new List<Ingredient> { Stock(1, "Coconut milk", 400, MeasurementUnit.ml, available: 2, canned: true) };

        Apply(stock, Need("Coconut milk", 0.3m, MeasurementUnit.l));

        Assert.That(stock.Single().Available, Is.EqualTo(1));
    }

    [Test]
    public void Canned_and_loose_entries_of_same_name_mix()
    {
        var stock = new List<Ingredient>
        {
            Stock(1, "Tomatoes", 400, MeasurementUnit.g, available: 2, canned: true),
            Stock(2, "Tomatoes", 100, MeasurementUnit.g),
        };

        var line = Apply(stock, Need("Tomatoes", 300, MeasurementUnit.g));

        // Smallest (loose 100 g) first, then one can for the remaining 200 g.
        Assert.That(line.Outcome, Is.EqualTo(UsageOutcome.Deducted));
        Assert.That(stock.Single().Available, Is.EqualTo(1));
        Assert.That(stock.Single().Canned, Is.True);
    }
}
