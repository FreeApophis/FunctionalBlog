namespace FunctionalBlog.Application.Recipes;

public interface IRecipeRepository
{
    ValueTask<IReadOnlyList<Recipe>> All();

    ValueTask<Recipe?> Find(RecipeId id);

    ValueTask<RecipeId> NextId();

    ValueTask Save(Recipe recipe);

    ValueTask Delete(RecipeId id);
}
