namespace FunctionalBlog.Recipes;

public static class RecipeHandlers
{
    public static App Index => _ => async env =>
    {
        var recipes = await env.Recipes.All();
        var users = await env.Users.All();
        var authorNames = users.ToDictionary(u => u.Id, u => u.DisplayName.Value);
        return Response.Html(RecipeViews.Index(recipes, env.CurrentUser, authorNames, env.T));
    };

    public static App ShowRecipe(RecipeId id) => _ => async env =>
    {
        var recipe = await env.Recipes.Find(id);

        if (recipe is null)
        {
            return Response.NotFound();
        }

        var author = await env.Users.FindById(recipe.AuthorId);
        var authorName = author?.DisplayName.Value ?? "?";
        var ingredients = (await env.Ingredients.All()).ToDictionary(i => i.Id);
        return Response.Html(RecipeViews.Show(recipe, env.CurrentUser, authorName, ingredients, env.T));
    };
}
