using System.Collections.Concurrent;

namespace FunctionalBlog.DataAccess.Recipes;

public sealed class InMemoryRecipeRepository : IRecipeRepository
{
    private readonly ConcurrentDictionary<int, Recipe> _recipes = new();
    private int _nextId = 1;

    public ValueTask<IReadOnlyList<Recipe>> All() =>
        ValueTask.FromResult<IReadOnlyList<Recipe>>(
            _recipes.Values.OrderBy(x => x.Name.Value).ToList());

    public ValueTask<Option<Recipe>> Find(RecipeId id) =>
        ValueTask.FromResult(_recipes.TryGetValue(id.Value, out var recipe) ? Option.Some(recipe) : Option<Recipe>.None);

    public ValueTask<RecipeId> NextId() =>
        ValueTask.FromResult(new RecipeId(Interlocked.Increment(ref _nextId)));

    public ValueTask Save(Recipe recipe)
    {
        _recipes[recipe.Id.Value] = recipe;
        return ValueTask.CompletedTask;
    }

    public ValueTask Delete(RecipeId id)
    {
        _recipes.TryRemove(id.Value, out _);
        return ValueTask.CompletedTask;
    }
}
