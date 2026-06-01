using System.Globalization;

namespace FunctionalBlog.Recipes;

public static class RecipeForm
{
    public static DecodedRecipeForm Decode(Request request)
    {
        var name = request.Form.GetValueOrDefault("name", string.Empty).Trim();
        var description = request.Form.GetValueOrDefault("description", string.Empty).Trim();
        var portionsRaw = request.Form.GetValueOrDefault("portions", string.Empty).Trim();
        var difficultyRaw = request.Form.GetValueOrDefault("difficulty", string.Empty).Trim();
        var tagsRaw = request.Form.GetValueOrDefault("tags", string.Empty).Trim();
        var hintsRaw = request.Form.GetValueOrDefault("hints", string.Empty).Trim();

        var ingredients = ParseIngredients(request);
        var rawSteps = ParseRawSteps(request);

        var errors = new List<string>();

        if (name.Length < 3)
        {
            errors.Add("recipe.error.name_too_short");
        }

        if (description.Length < 10)
        {
            errors.Add("recipe.error.description_too_short");
        }

        if (!int.TryParse(portionsRaw, out var portions) || portions < 1)
        {
            errors.Add("recipe.error.portions_invalid");
        }

        if (!int.TryParse(difficultyRaw, out var difficultyInt) || !Enum.IsDefined(typeof(Difficulty), difficultyInt))
        {
            errors.Add("recipe.error.difficulty_invalid");
        }

        var nonEmptySteps = rawSteps.Where(s => s.Length > 0).ToList();
        if (nonEmptySteps.Count == 0)
        {
            errors.Add("recipe.error.no_steps");
        }

        foreach (var (id, amount, unit) in ingredients)
        {
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            if (!decimal.TryParse(amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt) || amt <= 0 || ParseUnit(unit) is null)
            {
                errors.Add("recipe.error.ingredient_invalid");
                break;
            }
        }

        return new DecodedRecipeForm(
            IsValid: errors.Count == 0,
            Errors: errors,
            Name: name,
            Description: description,
            Portions: portionsRaw,
            Difficulty: difficultyRaw,
            Tags: tagsRaw,
            Hints: hintsRaw,
            Ingredients: ingredients,
            Steps: rawSteps);
    }

    public static List<(string Id, string Amount, string Unit)> ParseIngredients(Request request)
    {
        var result = new List<(string, string, string)>();
        for (var i = 0; request.Form.ContainsKey($"ingredient_id_{i}"); i++)
        {
            result.Add((
                request.Form.GetValueOrDefault($"ingredient_id_{i}", string.Empty),
                request.Form.GetValueOrDefault($"ingredient_amount_{i}", string.Empty),
                request.Form.GetValueOrDefault($"ingredient_unit_{i}", string.Empty)));
        }

        return result;
    }

    public static List<string> ParseRawSteps(Request request)
    {
        var result = new List<string>();
        for (var i = 0; request.Form.ContainsKey($"step_{i}"); i++)
        {
            result.Add(request.Form[$"step_{i}"].Trim());
        }

        return result;
    }

    public static Unit? ParseUnit(string abbreviation) => abbreviation switch
    {
        "g" => WeightUnit.Gram,
        "kg" => WeightUnit.Kilogram,
        "ml" => VolumeUnit.Milliliter,
        "dl" => VolumeUnit.Deciliter,
        "l" => VolumeUnit.Liter,
        "EL" => VolumeUnit.Tablespoon,
        "TL" => VolumeUnit.Teaspoon,
        "Stück" => PieceUnit.Piece,
        "Prise" => PieceUnit.Pinch,
        _ => null,
    };

    public static IReadOnlyList<(string Name, string Abbreviation)> AllUnits =>
    [
        ("Gramm", "g"),
        ("Kilogramm", "kg"),
        ("Milliliter", "ml"),
        ("Deziliter", "dl"),
        ("Liter", "l"),
        ("Esslöffel", "EL"),
        ("Teelöffel", "TL"),
        ("Stück", "Stück"),
        ("Prise", "Prise"),
    ];
}
