namespace FunctionalBlog.Test;

public sealed class DomainEqualityTests
{
    [Fact]
    public void Two_users_with_same_role_names_are_equal()
    {
        var a = User.Create(new UserId(1), new Email("a@b.de"), new DisplayName("X"), "hash", ["Admin"], DateTimeOffset.MinValue);
        var b = User.Create(new UserId(1), new Email("a@b.de"), new DisplayName("X"), "hash", ["Admin"], DateTimeOffset.MinValue);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Two_roles_with_same_rules_are_equal()
    {
        var rule = new PermissionRule("Create", "article");
        var a = Role.Create(new RoleId(1), "Admin").AddRule(rule);
        var b = Role.Create(new RoleId(1), "Admin").AddRule(rule);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Two_recipes_with_same_steps_and_ingredients_are_equal()
    {
        var id = new RecipeId(1);
        var a = Recipe.Create(
            id,
            new RecipeName("Kuchen"),
            new RecipeDescription("Beschreibung"),
            [new PreparationStep(1, "Backen.")],
            new UserId(1),
            Difficulty.Easy,
            [new RecipeTag("Backen")],
            4,
            [new RecipeIngredient(new IngredientId(1), 200m, WeightUnit.Gram)],
            ["http://img.example.com/1.jpg"],
            [new RecipeHint("Tipp!")]);
        var b = Recipe.Create(
            id,
            new RecipeName("Kuchen"),
            new RecipeDescription("Beschreibung"),
            [new PreparationStep(1, "Backen.")],
            new UserId(1),
            Difficulty.Easy,
            [new RecipeTag("Backen")],
            4,
            [new RecipeIngredient(new IngredientId(1), 200m, WeightUnit.Gram)],
            ["http://img.example.com/1.jpg"],
            [new RecipeHint("Tipp!")]);

        Assert.Equal(a, b);
    }
}
