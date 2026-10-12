using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RecipeIngredients.Models;

namespace RecipeIngredients.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Staple> Staples => Set<Staple>();
    public DbSet<Recipe> Recipes => Set<Recipe>();

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ingredient>()
            .Property(i => i.Unit)
            .HasConversion<string>();

        modelBuilder.Entity<Recipe>(b =>
        {
            b.Property(r => r.Ingredients)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOpts),
                    v => JsonSerializer.Deserialize<List<RecipeIngredient>>(v, JsonOpts) ?? new List<RecipeIngredient>())
                .HasColumnType("jsonb");

            b.Property(r => r.Steps)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOpts),
                    v => JsonSerializer.Deserialize<List<string>>(v, JsonOpts) ?? new List<string>())
                .HasColumnType("jsonb");

            b.Property(r => r.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOpts),
                    v => JsonSerializer.Deserialize<List<string>>(v, JsonOpts) ?? new List<string>())
                .HasColumnType("jsonb");
        });
    }
}
