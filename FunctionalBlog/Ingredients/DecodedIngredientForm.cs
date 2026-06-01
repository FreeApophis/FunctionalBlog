namespace FunctionalBlog.Ingredients;

public sealed record DecodedIngredientForm(
    bool IsValid,
    IReadOnlyList<string> Errors,
    string Name,
    string Description,
    string Image,
    string Density,
    string PieceCount,
    string CalorificValue,
    string Protein,
    string Fat,
    string Carbohydrates,
    string Sugar,
    string Fiber);
