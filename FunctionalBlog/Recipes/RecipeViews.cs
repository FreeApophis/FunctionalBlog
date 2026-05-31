namespace FunctionalBlog.Recipes;

public static class RecipeViews
{
    public static string Index(
        IReadOnlyList<Recipe> recipes,
        IPrincipal principal,
        IReadOnlyDictionary<UserId, string> authorNames,
        Translate t)
    {
        string RecipeHtml(Recipe recipe)
        {
            var authorName = authorNames.TryGetValue(recipe.AuthorId, out var name) ? name : "?";
            var content = Html.H2(Html.Link($"/recipes/{recipe.Id.Value}", recipe.Name.Value)) +
                Html.Small($"{t("recipe.by")} {Html.Encode(authorName)} · {t(DifficultyKey(recipe.Difficulty))} · {recipe.Portions} {t("recipe.portions")}") +
                Html.P(Html.Encode(recipe.Description.Value));
            return Html.Article(content);
        }

        var items = recipes.Count == 0
            ? Html.P(t("recipe.no_recipes"))
            : string.Join(string.Empty, recipes.Select(RecipeHtml));

        var body = Html.H1(t("recipe.title")) + items;

        return Layout.Page(t("recipe.title"), body, principal, t);
    }

    public static string Show(
        Recipe recipe,
        IPrincipal principal,
        string authorName,
        IReadOnlyDictionary<IngredientId, Ingredient> ingredientMap,
        Translate t)
    {
        var tags = recipe.Tags.Count > 0
            ? Html.P(string.Join(", ", recipe.Tags.Select(tag => Html.Encode(tag.Value))))
            : string.Empty;

        var images = recipe.Images.Count > 0
            ? string.Concat(recipe.Images.Select(url => $"""<img src="{Html.Encode(url)}" alt="{Html.Encode(recipe.Name.Value)}" />"""))
            : string.Empty;

        var steps = recipe.PreparationSteps.Count > 0
            ? "<ol>" + string.Concat(
                recipe.PreparationSteps
                    .OrderBy(s => s.SortOrder)
                    .Select(s => $"<li>{Html.Encode(s.Text)}</li>")) + "</ol>"
            : Html.P(t("recipe.no_steps"));

        var ingredientRows = string.Concat(recipe.Ingredients.Select(ri =>
        {
            var name = ingredientMap.TryGetValue(ri.IngredientId, out var ing)
                ? Html.Encode(ing.Name.Value)
                : "?";
            return $"<tr><td>{ri.Amount:G29} {Html.Encode(ri.Unit.Abbreviation)}</td><td>{name}</td></tr>";
        }));
        var ingredientTable = recipe.Ingredients.Count > 0
            ? $"<table><tbody>{ingredientRows}</tbody></table>"
            : string.Empty;

        var hints = recipe.Hints.Count > 0
            ? Html.Ul(recipe.Hints.Select(h => Html.Encode(h.Text)))
            : string.Empty;

        var meta = Html.Small(
            $"{t("recipe.by")} {Html.Encode(authorName)} · " +
            $"{t(DifficultyKey(recipe.Difficulty))} · " +
            $"{recipe.Portions} {t("recipe.portions")}");

        var body = Html.P(Html.Link("/recipes", t("common.back"))) +
            Html.H1(recipe.Name.Value) +
            meta +
            tags +
            images +
            Html.P(Html.Encode(recipe.Description.Value)) +
            Html.H2(t("recipe.preparation")) +
            steps +
            Html.H2(t("recipe.ingredients")) +
            ingredientTable +
            (recipe.Hints.Count > 0 ? Html.H2(t("recipe.hints")) + hints : string.Empty);

        return Layout.Page(recipe.Name.Value, body, principal, t);
    }

    private static string DifficultyKey(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => "recipe.difficulty.easy",
        Difficulty.Medium => "recipe.difficulty.medium",
        Difficulty.Hard => "recipe.difficulty.hard",
        Difficulty.Expert => "recipe.difficulty.expert",
        _ => difficulty.ToString(),
    };
}
