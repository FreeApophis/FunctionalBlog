namespace FunctionalBlog.Test.Ingredients;

public sealed class IngredientFormTests
{
    [Fact]
    public void Valid_form_is_valid()
    {
        var request = Build("Ei", "Hühnerei", "1.0", "1.0", "155", "13", "11", "1", "0.4", "0");

        var decoded = IngredientForm.Decode(request);

        Assert.True(decoded.IsValid);
        Assert.Empty(decoded.Errors);
    }

    [Fact]
    public void Name_shorter_than_2_characters_adds_error()
    {
        var request = Build("X", "Beschreibung", "1.0", "0", "0", "0", "0", "0", "0", "0");

        var decoded = IngredientForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("ingredient.error.name_too_short", decoded.Errors);
    }

    [Fact]
    public void Name_of_2_characters_is_valid()
    {
        var request = Build("Ei", "Beschreibung", "1.0", "0", "0", "0", "0", "0", "0", "0");

        var decoded = IngredientForm.Decode(request);

        Assert.True(decoded.IsValid);
    }

    [Fact]
    public void Density_of_zero_or_less_adds_error()
    {
        var request = Build("Salz", "Beschreibung", "0", "0", "0", "0", "0", "0", "0", "0");

        var decoded = IngredientForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("ingredient.error.density_invalid", decoded.Errors);
    }

    [Fact]
    public void Density_not_a_number_adds_error()
    {
        var request = Build("Salz", "Beschreibung", "abc", "0", "0", "0", "0", "0", "0", "0");

        var decoded = IngredientForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("ingredient.error.density_invalid", decoded.Errors);
    }

    [Fact]
    public void Negative_piece_count_adds_error()
    {
        var request = Build("Ei", "Beschreibung", "1.0", "-1", "0", "0", "0", "0", "0", "0");

        var decoded = IngredientForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("ingredient.error.piece_count_invalid", decoded.Errors);
    }

    [Fact]
    public void Negative_calorific_value_adds_error()
    {
        var request = Build("Ei", "Beschreibung", "1.0", "0", "-1", "0", "0", "0", "0", "0");

        var decoded = IngredientForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("ingredient.error.calorific_value_invalid", decoded.Errors);
    }

    [Fact]
    public void Optional_image_and_description_may_be_empty()
    {
        var request = BuildFull(
            name: "Öl",
            description: string.Empty,
            image: string.Empty,
            density: "0.92",
            pieceCount: "0",
            calorificValue: "900",
            protein: "0",
            fat: "100",
            carbohydrates: "0",
            sugar: "0",
            fiber: "0");

        var decoded = IngredientForm.Decode(request);

        Assert.True(decoded.IsValid);
    }

    [Fact]
    public void All_nutrient_fields_are_preserved_in_decoded_form()
    {
        var request = Build("Butter", "Süßrahmbutter", "0.96", "0", "740", "0.7", "83", "0.4", "0.4", "0");

        var decoded = IngredientForm.Decode(request);

        Assert.Equal("Butter", decoded.Name);
        Assert.Equal("0.96", decoded.Density);
        Assert.Equal("740", decoded.CalorificValue);
        Assert.Equal("83", decoded.Fat);
    }

    private static Request Build(
        string name,
        string description,
        string density,
        string pieceCount,
        string calorificValue,
        string protein,
        string fat,
        string carbohydrates,
        string sugar,
        string fiber) =>
        BuildFull(name, description, string.Empty, density, pieceCount, calorificValue, protein, fat, carbohydrates, sugar, fiber);

    private static Request BuildFull(
        string name,
        string description,
        string image,
        string density,
        string pieceCount,
        string calorificValue,
        string protein,
        string fat,
        string carbohydrates,
        string sugar,
        string fiber)
    {
        var form = new Dictionary<string, string>
        {
            ["name"] = name,
            ["description"] = description,
            ["image"] = image,
            ["density"] = density,
            ["piece_count"] = pieceCount,
            ["calorific_value"] = calorificValue,
            ["protein"] = protein,
            ["fat"] = fat,
            ["carbohydrates"] = carbohydrates,
            ["sugar"] = sugar,
            ["fiber"] = fiber,
        };
        return new Request(HttpMethod.Post, "/admin/ingredients", Empty, Empty, form, Empty);
    }

    private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();
}
