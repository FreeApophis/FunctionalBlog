namespace FunctionalBlog.Test.Recipes;

public sealed class UnitTests
{
    [Fact]
    public void WeightUnit_Gram_is_the_base_unit_with_factor_1()
    {
        Assert.Equal(1m, WeightUnit.Gram.Factor);
    }

    [Fact]
    public void VolumeUnit_Milliliter_is_the_base_unit_with_factor_1()
    {
        Assert.Equal(1m, VolumeUnit.Milliliter.Factor);
    }

    [Fact]
    public void PieceUnit_Piece_is_the_base_unit_with_factor_1()
    {
        Assert.Equal(1m, PieceUnit.Piece.Factor);
    }

    [Fact]
    public void WeightUnit_Convert_grams_to_kilograms()
    {
        var result = WeightUnit.Convert(250m, WeightUnit.Gram, WeightUnit.Kilogram);

        Assert.Equal(0.25m, result);
    }

    [Fact]
    public void WeightUnit_Convert_kilograms_to_grams()
    {
        var result = WeightUnit.Convert(0.5m, WeightUnit.Kilogram, WeightUnit.Gram);

        Assert.Equal(500m, result);
    }

    [Fact]
    public void VolumeUnit_Convert_liters_to_milliliters()
    {
        var result = VolumeUnit.Convert(0.4m, VolumeUnit.Liter, VolumeUnit.Milliliter);

        Assert.Equal(400m, result);
    }

    [Fact]
    public void VolumeUnit_Convert_tablespoons_to_milliliters()
    {
        var result = VolumeUnit.Convert(2m, VolumeUnit.Tablespoon, VolumeUnit.Milliliter);

        Assert.Equal(30m, result);
    }

    [Fact]
    public void WeightUnit_has_correct_abbreviations()
    {
        Assert.Equal("g", WeightUnit.Gram.Abbreviation);
        Assert.Equal("kg", WeightUnit.Kilogram.Abbreviation);
    }

    [Fact]
    public void VolumeUnit_has_correct_abbreviations()
    {
        Assert.Equal("ml", VolumeUnit.Milliliter.Abbreviation);
        Assert.Equal("l", VolumeUnit.Liter.Abbreviation);
        Assert.Equal("EL", VolumeUnit.Tablespoon.Abbreviation);
        Assert.Equal("TL", VolumeUnit.Teaspoon.Abbreviation);
    }

    [Fact]
    public void WeightUnit_and_VolumeUnit_with_same_values_are_not_equal()
    {
        var weight = new WeightUnit("X", "x", 1m);
        var volume = new VolumeUnit("X", "x", 1m);

        Assert.NotEqual<FunctionalBlog.Domain.Recipes.Unit>(weight, volume);
    }

    [Fact]
    public void All_contains_nine_units()
    {
        Assert.Equal(9, FunctionalBlog.Domain.Recipes.Unit.All.Count);
    }

    [Fact]
    public void ParseByAbbreviation_round_trips_for_every_unit_in_All()
    {
        foreach (var unit in FunctionalBlog.Domain.Recipes.Unit.All)
        {
            var result = FunctionalBlog.Domain.Recipes.Unit.ParseByAbbreviation(unit.Abbreviation);
            Assert.Equal(Option.Some(unit), result);
        }
    }

    [Fact]
    public void ParseByAbbreviation_returns_None_for_unknown_abbreviation()
    {
        var result = FunctionalBlog.Domain.Recipes.Unit.ParseByAbbreviation("unknown");
        FunctionalAssert.None(result);
    }
}
