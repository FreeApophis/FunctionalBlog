namespace FunctionalBlog.Test.Recipes;

public sealed class RecipePdfMapperTests
{
    private static readonly Translate Echo = key => key;

    [Fact]
    public void Create_keeps_base_amounts_for_the_base_portions()
    {
        var model = RecipePdfMapper.Create(ARecipe(basePortions: 2, amount: 200m), 2, "Sabrina", Names(), null, Echo);

        Assert.Equal(2, model.Portions);
        Assert.StartsWith("200", Assert.Single(model.Ingredients).Amount);
    }

    [Fact]
    public void Create_scales_ingredient_amounts_to_the_requested_portions()
    {
        var model = RecipePdfMapper.Create(ARecipe(basePortions: 2, amount: 200m), 4, "Sabrina", Names(), null, Echo);

        Assert.Equal(4, model.Portions);
        Assert.StartsWith("400", Assert.Single(model.Ingredients).Amount);
    }

    private static IReadOnlyDictionary<IngredientId, string> Names() =>
        new Dictionary<IngredientId, string> { [new IngredientId(1)] = "Mehl" };

    private static Recipe ARecipe(int basePortions, decimal amount) =>
        Recipe.Create(
            new RecipeId(1),
            new RecipeName("Crispy Chicken"),
            new RecipeDescription("Lecker."),
            [new PreparationStep(1, "Kochen.")],
            new UserId(1),
            Difficulty.Easy,
            [],
            basePortions,
            [new RecipeIngredient(new IngredientId(1), amount, Gram)],
            [],
            []);
}
