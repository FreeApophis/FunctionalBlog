namespace FunctionalBlog.Application.Slugs;

// Ensures every existing content entity has a slug in the registry. Runs at startup and is
// idempotent (SlugService.Ensure keeps an entity's existing slug), so it safely picks up any
// rows created before the slug write-path existed. Tag slugs are handled separately.
public static class SlugBackfill
{
    public static async Task Run(
        SlugService slugs,
        IArticleRepository articles,
        IRecipeRepository recipes,
        IPageRepository pages,
        IIngredientRepository ingredients)
    {
        foreach (var article in await articles.All())
        {
            await slugs.Ensure(SlugEntityType.Article, article.Id.Value, article.Title.Value);
        }

        foreach (var recipe in await recipes.All())
        {
            await slugs.Ensure(SlugEntityType.Recipe, recipe.Id.Value, recipe.Name.Value);
        }

        foreach (var page in await pages.All())
        {
            await slugs.Ensure(SlugEntityType.Page, page.Id.Value, page.Title.Value);
        }

        foreach (var ingredient in await ingredients.All())
        {
            await slugs.Ensure(SlugEntityType.Ingredient, ingredient.Id.Value, ingredient.Name.Value);
        }
    }
}
