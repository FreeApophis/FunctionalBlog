using System.Globalization;

namespace FunctionalBlog.Ingredients;

public static class IngredientForm
{
    public static DecodedIngredientForm Decode(Request request)
    {
        var name = request.Form.GetValueOrDefault("name", string.Empty).Trim();
        var description = request.Form.GetValueOrDefault("description", string.Empty).Trim();
        var image = request.Form.GetValueOrDefault("image", string.Empty).Trim();
        var density = request.Form.GetValueOrDefault("density", string.Empty).Trim();
        var pieceCount = request.Form.GetValueOrDefault("piece_count", string.Empty).Trim();
        var calorificValue = request.Form.GetValueOrDefault("calorific_value", string.Empty).Trim();
        var protein = request.Form.GetValueOrDefault("protein", string.Empty).Trim();
        var fat = request.Form.GetValueOrDefault("fat", string.Empty).Trim();
        var carbohydrates = request.Form.GetValueOrDefault("carbohydrates", string.Empty).Trim();
        var sugar = request.Form.GetValueOrDefault("sugar", string.Empty).Trim();
        var fiber = request.Form.GetValueOrDefault("fiber", string.Empty).Trim();

        var errors = new List<string>();

        if (name.Length < 2)
        {
            errors.Add("ingredient.error.name_too_short");
        }

        if (!decimal.TryParse(density, NumberStyles.Any, CultureInfo.InvariantCulture, out var densityVal) || densityVal <= 0)
        {
            errors.Add("ingredient.error.density_invalid");
        }

        if (!decimal.TryParse(pieceCount, NumberStyles.Any, CultureInfo.InvariantCulture, out var pieceCountVal) || pieceCountVal < 0)
        {
            errors.Add("ingredient.error.piece_count_invalid");
        }

        if (!decimal.TryParse(calorificValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var calorificVal) || calorificVal < 0)
        {
            errors.Add("ingredient.error.calorific_value_invalid");
        }

        foreach (var (field, key) in NutrientFields(protein, fat, carbohydrates, sugar, fiber))
        {
            if (!decimal.TryParse(field, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) || val < 0)
            {
                errors.Add(key);
            }
        }

        return new DecodedIngredientForm(
            IsValid: errors.Count == 0,
            Errors: errors,
            Name: name,
            Description: description,
            Image: image,
            Density: density,
            PieceCount: pieceCount,
            CalorificValue: calorificValue,
            Protein: protein,
            Fat: fat,
            Carbohydrates: carbohydrates,
            Sugar: sugar,
            Fiber: fiber);
    }

    private static IEnumerable<(string Value, string ErrorKey)> NutrientFields(
        string protein,
        string fat,
        string carbohydrates,
        string sugar,
        string fiber)
    {
        yield return (protein, "ingredient.error.protein_invalid");
        yield return (fat, "ingredient.error.fat_invalid");
        yield return (carbohydrates, "ingredient.error.carbohydrates_invalid");
        yield return (sugar, "ingredient.error.sugar_invalid");
        yield return (fiber, "ingredient.error.fiber_invalid");
    }
}
