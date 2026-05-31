namespace FunctionalBlog.Domain.Recipes;

public sealed record Recipe(
    RecipeId Id,
    RecipeName Name,
    RecipeDescription Description,
    IReadOnlyList<PreparationStep> PreparationSteps,
    UserId AuthorId,
    Difficulty Difficulty,
    IReadOnlyList<RecipeTag> Tags,
    int Portions,
    IReadOnlyList<RecipeIngredient> Ingredients,
    IReadOnlyList<string> Images,
    IReadOnlyList<RecipeHint> Hints)
{
    public static Recipe Create(
        RecipeId id,
        RecipeName name,
        RecipeDescription description,
        IReadOnlyList<PreparationStep> preparationSteps,
        UserId authorId,
        Difficulty difficulty,
        IReadOnlyList<RecipeTag> tags,
        int portions,
        IReadOnlyList<RecipeIngredient> ingredients,
        IReadOnlyList<string> images,
        IReadOnlyList<RecipeHint> hints) =>
        new(id, name, description, preparationSteps, authorId, difficulty, tags, portions, ingredients, images, hints);
}
