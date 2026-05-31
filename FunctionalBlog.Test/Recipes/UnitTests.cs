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

        Assert.NotEqual<Unit>(weight, volume);
    }
}
