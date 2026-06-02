namespace FunctionalBlog.Test.Recipes;

public abstract class RecipeRepositoryContract
{
    [Fact]
    public async Task Save_then_Find_returns_the_saved_recipe()
    {
        var repo = CreateRepository();
        var id = await repo.NextId();
        var recipe = ARecipe(id);

        await repo.Save(recipe);

        Assert.Equal(Option.Some(recipe), await repo.Find(id));
    }

    [Fact]
    public async Task Find_returns_null_for_an_unknown_id()
    {
        var repo = CreateRepository();

        Assert.Equal(Option<Recipe>.None, await repo.Find(new RecipeId(987_654)));
    }

    [Fact]
    public async Task NextId_returns_an_id_that_does_not_yet_exist()
    {
        var repo = CreateRepository();

        var id = await repo.NextId();

        Assert.Equal(Option<Recipe>.None, await repo.Find(id));
    }

    [Fact]
    public async Task NextId_returns_distinct_values_across_calls()
    {
        var repo = CreateRepository();

        var first = await repo.NextId();
        var second = await repo.NextId();

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task All_returns_all_saved_recipes()
    {
        var repo = CreateRepository();
        var first = ARecipe(await repo.NextId(), name: "Pasta");
        var second = ARecipe(await repo.NextId(), name: "Pizza");

        await repo.Save(first);
        await repo.Save(second);

        var all = await repo.All();
        Assert.Contains(first, all);
        Assert.Contains(second, all);
    }

    [Fact]
    public async Task Save_replaces_an_existing_recipe_with_the_same_id()
    {
        var repo = CreateRepository();
        var id = await repo.NextId();
        var original = ARecipe(id, name: "Original");
        var updated = ARecipe(id, name: "Aktualisiert");

        await repo.Save(original);
        await repo.Save(updated);

        Assert.Equal(Option.Some(updated), await repo.Find(id));
    }

    [Fact]
    public async Task Delete_removes_the_recipe()
    {
        var repo = CreateRepository();
        var id = await repo.NextId();
        await repo.Save(ARecipe(id));

        await repo.Delete(id);

        Assert.Equal(Option<Recipe>.None, await repo.Find(id));
    }

    [Fact]
    public async Task Delete_is_idempotent_for_unknown_id()
    {
        var repo = CreateRepository();

        await repo.Delete(new RecipeId(987_654));
    }

    protected abstract IRecipeRepository CreateRepository();

    private static Recipe ARecipe(RecipeId id, string name = "Rezept") =>
        Recipe.Create(
            id,
            new RecipeName(name),
            new RecipeDescription("Eine Beschreibung"),
            [],
            new UserId(1),
            Difficulty.Medium,
            [],
            2,
            [],
            [],
            []);
}
