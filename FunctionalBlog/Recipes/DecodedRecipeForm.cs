namespace FunctionalBlog.Recipes;

public sealed record DecodedRecipeForm(
    bool IsValid,
    IReadOnlyList<string> Errors,
    string Name,
    string Description,
    string Portions,
    string Difficulty,
    string Tags,
    string Hints,
    IReadOnlyList<(string Id, string Amount, string Unit)> Ingredients,
    IReadOnlyList<string> Steps);
