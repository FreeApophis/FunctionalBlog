namespace FunctionalBlog.Test.Recipes;

public sealed class RecipeFormTests
{
    [Fact]
    public void Valid_form_is_valid()
    {
        var request = Build(
            "Rührkuchen",
            "Ein klassischer Rührkuchen.",
            "4",
            "0",
            ingredients: [("1", "200", "g")],
            steps: ["Alles verrühren."]);

        var decoded = RecipeForm.Decode(request);

        Assert.True(decoded.IsValid);
        Assert.Empty(decoded.Errors);
    }

    [Fact]
    public void Name_shorter_than_3_characters_adds_error()
    {
        var request = Build("AB", "Ein klassischer Rührkuchen.", "4", "0");

        var decoded = RecipeForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("recipe.error.name_too_short", decoded.Errors);
    }

    [Fact]
    public void Description_shorter_than_10_characters_adds_error()
    {
        var request = Build("Rührkuchen", "Zu kurz", "4", "0");

        var decoded = RecipeForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("recipe.error.description_too_short", decoded.Errors);
    }

    [Fact]
    public void Portions_zero_adds_error()
    {
        var request = Build("Rührkuchen", "Ein klassischer Rührkuchen.", "0", "0");

        var decoded = RecipeForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("recipe.error.portions_invalid", decoded.Errors);
    }

    [Fact]
    public void Portions_not_a_number_adds_error()
    {
        var request = Build("Rührkuchen", "Ein klassischer Rührkuchen.", "abc", "0");

        var decoded = RecipeForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("recipe.error.portions_invalid", decoded.Errors);
    }

    [Fact]
    public void Invalid_difficulty_adds_error()
    {
        var request = Build("Rührkuchen", "Ein klassischer Rührkuchen.", "4", "99");

        var decoded = RecipeForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("recipe.error.difficulty_invalid", decoded.Errors);
    }

    [Fact]
    public void No_steps_adds_error()
    {
        var request = Build(
            "Rührkuchen",
            "Ein klassischer Rührkuchen.",
            "4",
            "0",
            steps: []);

        var decoded = RecipeForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("recipe.error.no_steps", decoded.Errors);
    }

    [Fact]
    public void Only_whitespace_steps_add_error()
    {
        var request = Build(
            "Rührkuchen",
            "Ein klassischer Rührkuchen.",
            "4",
            "0",
            steps: ["   "]);

        var decoded = RecipeForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("recipe.error.no_steps", decoded.Errors);
    }

    [Fact]
    public void Invalid_ingredient_row_adds_error()
    {
        var request = Build(
            "Rührkuchen",
            "Ein klassischer Rührkuchen.",
            "4",
            "0",
            ingredients: [("1", "abc", "g")],
            steps: ["Backen."]);

        var decoded = RecipeForm.Decode(request);

        Assert.False(decoded.IsValid);
        Assert.Contains("recipe.error.ingredient_invalid", decoded.Errors);
    }

    [Fact]
    public void Empty_ingredient_rows_are_ignored_during_validation()
    {
        var request = Build(
            "Rührkuchen",
            "Ein klassischer Rührkuchen.",
            "4",
            "0",
            ingredients: [(string.Empty, string.Empty, "g")],
            steps: ["Backen."]);

        var decoded = RecipeForm.Decode(request);

        Assert.True(decoded.IsValid);
    }

    [Fact]
    public void ParseIngredients_returns_all_indexed_rows()
    {
        var request = Build(
            "X",
            "X",
            "1",
            "0",
            ingredients: [("2", "100", "g"), ("5", "1.5", "EL")]);

        var ings = RecipeForm.ParseIngredients(request);

        Assert.Equal(2, ings.Count);
        Assert.Equal(("2", "100", "g"), ings[0]);
        Assert.Equal(("5", "1.5", "EL"), ings[1]);
    }

    [Fact]
    public void ParseRawSteps_returns_all_indexed_steps_including_empty()
    {
        var request = Build(
            "X",
            "X",
            "1",
            "0",
            steps: ["Erster Schritt.", string.Empty, "Dritter Schritt."]);

        var steps = RecipeForm.ParseRawSteps(request);

        Assert.Equal(3, steps.Count);
        Assert.Equal("Erster Schritt.", steps[0]);
        Assert.Equal(string.Empty, steps[1]);
        Assert.Equal("Dritter Schritt.", steps[2]);
    }

    [Fact]
    public void ParseUnit_returns_correct_units()
    {
        Assert.Equal(WeightUnit.Gram, RecipeForm.ParseUnit("g"));
        Assert.Equal(WeightUnit.Kilogram, RecipeForm.ParseUnit("kg"));
        Assert.Equal(VolumeUnit.Milliliter, RecipeForm.ParseUnit("ml"));
        Assert.Equal(VolumeUnit.Tablespoon, RecipeForm.ParseUnit("EL"));
        Assert.Equal(PieceUnit.Piece, RecipeForm.ParseUnit("Stück"));
        Assert.Null(RecipeForm.ParseUnit("unknown"));
    }

    private static Request Build(
        string name,
        string description,
        string portions,
        string difficulty,
        IReadOnlyList<(string Id, string Amount, string Unit)>? ingredients = null,
        IReadOnlyList<string>? steps = null)
    {
        var form = new Dictionary<string, string>
        {
            ["name"] = name,
            ["description"] = description,
            ["portions"] = portions,
            ["difficulty"] = difficulty,
        };

        foreach (var (i, (id, amount, unit)) in (ingredients ?? []).Select((x, i) => (i, x)))
        {
            form[$"ingredient_id_{i}"] = id;
            form[$"ingredient_amount_{i}"] = amount;
            form[$"ingredient_unit_{i}"] = unit;
        }

        foreach (var (i, step) in (steps ?? []).Select((s, i) => (i, s)))
        {
            form[$"step_{i}"] = step;
        }

        return new Request("POST", "/recipes", Empty, Empty, form, Empty);
    }

    private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();
}
