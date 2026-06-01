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

        var body = Html.H1(t("recipe.title")) +
            (principal.Can<Create>(new RecipeResource())
                ? Html.P(Html.Link("/recipes/new", t("recipe.new_recipe")))
                : string.Empty) +
            items;

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

    public static string Form(
        IReadOnlyList<string> errors,
        string name,
        string description,
        string portions,
        string difficulty,
        string tags,
        string hints,
        IReadOnlyList<(string Id, string Amount, string Unit)> ingredients,
        IReadOnlyList<string> steps,
        IReadOnlyList<Ingredient> availableIngredients,
        IPrincipal principal,
        Translate t)
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(key => t(key))));

        var difficultyOptions = string.Concat(Enum.GetValues<Difficulty>().Select(d =>
        {
            var selected = ((int)d).ToString() == difficulty ? " selected" : string.Empty;
            return $"""<option value="{(int)d}"{selected}>{Html.Encode(t(DifficultyKey(d)))}</option>""";
        }));

        var form = $"""
            <form method="post" action="/recipes">
                <button type="submit" hidden></button>
                <label>
                    {Html.Encode(t("recipe.field.name"))}
                    <input name="name" value="{Html.Encode(name)}" />
                </label>
                <label>
                    {Html.Encode(t("recipe.field.description"))}
                    <textarea name="description" rows="3">{Html.Encode(description)}</textarea>
                </label>
                <label>
                    {Html.Encode(t("recipe.field.portions"))}
                    <input name="portions" type="number" min="1" value="{Html.Encode(portions)}" />
                </label>
                <label>
                    {Html.Encode(t("recipe.field.difficulty"))}
                    <select name="difficulty">{difficultyOptions}</select>
                </label>
                <label>
                    {Html.Encode(t("recipe.field.tags"))}
                    <input name="tags" value="{Html.Encode(tags)}" />
                </label>
                {IngredientSection(ingredients, availableIngredients, t)}
                {StepSection(steps, t)}
                <label>
                    {Html.Encode(t("recipe.field.hints"))}
                    <textarea name="hints" rows="4">{Html.Encode(hints)}</textarea>
                </label>
                <button type="submit">{Html.Encode(t("recipe.submit"))}</button>
            </form>
            """;

        var body = Html.P(Html.Link("/recipes", t("common.back"))) +
            Html.H1(t("recipe.new_title")) +
            errorHtml +
            form;

        return Layout.Page(t("recipe.new_title"), body, principal, t);
    }

    public static string IngredientSection(
        IReadOnlyList<(string Id, string Amount, string Unit)> ingredients,
        IReadOnlyList<Ingredient> availableIngredients,
        Translate t)
    {
        string IngredientRow(int i, string id, string amount, string unit)
        {
            var idOptions = $"""<option value="">{Html.Encode(t("recipe.select_ingredient"))}</option>""" +
                string.Concat(availableIngredients.Select(ing =>
                {
                    var selected = ing.Id.Value.ToString() == id ? " selected" : string.Empty;
                    return $"""<option value="{ing.Id.Value}"{selected}>{Html.Encode(ing.Name.Value)}</option>""";
                }));

            var unitOptions = string.Concat(RecipeForm.AllUnits.Select(u =>
            {
                var selected = u.Abbreviation == unit ? " selected" : string.Empty;
                return $"""<option value="{Html.Encode(u.Abbreviation)}"{selected}>{Html.Encode(u.Name)}</option>""";
            }));

            return $"""
                <div class="ingredient-row" id="ingredient-row-{i}">
                    <select name="ingredient_id_{i}">{idOptions}</select>
                    <input name="ingredient_amount_{i}" type="number" step="any" min="0" value="{Html.Encode(amount)}" />
                    <select name="ingredient_unit_{i}">{unitOptions}</select>
                    <button type="submit" name="action" value="remove-ingredient-{i}"
                            hx-post="/recipes/form/ingredients"
                            hx-target="#ingredients-section"
                            hx-swap="outerHTML"
                            hx-include="#ingredients-list input, #ingredients-list select">
                        {Html.Encode(t("recipe.remove"))}
                    </button>
                </div>
                """;
        }

        var rows = string.Concat(ingredients.Select((ing, i) => IngredientRow(i, ing.Id, ing.Amount, ing.Unit)));

        return $"""
            <div id="ingredients-section">
                <h3>{Html.Encode(t("recipe.field.ingredients"))}</h3>
                <div id="ingredients-list">{rows}</div>
                <button type="submit" name="action" value="add-ingredient"
                        hx-post="/recipes/form/ingredients"
                        hx-target="#ingredients-section"
                        hx-swap="outerHTML"
                        hx-include="#ingredients-list input, #ingredients-list select">
                    {Html.Encode(t("recipe.add_ingredient"))}
                </button>
            </div>
            """;
    }

    public static string StepSection(IReadOnlyList<string> steps, Translate t)
    {
        string StepRow(int i, string text)
        {
            var label = Html.Encode($"{t("recipe.field.step")} {i + 1}");
            return $"""
                <div class="step-row" id="step-row-{i}">
                    <label>{label}
                        <textarea name="step_{i}" rows="2">{Html.Encode(text)}</textarea>
                    </label>
                    <button type="submit" name="action" value="remove-step-{i}"
                            hx-post="/recipes/form/steps"
                            hx-target="#steps-section"
                            hx-swap="outerHTML"
                            hx-include="#steps-list textarea">
                        {Html.Encode(t("recipe.remove"))}
                    </button>
                </div>
                """;
        }

        var rows = string.Concat(steps.Select((s, i) => StepRow(i, s)));

        return $"""
            <div id="steps-section">
                <h3>{Html.Encode(t("recipe.field.steps"))}</h3>
                <div id="steps-list">{rows}</div>
                <button type="submit" name="action" value="add-step"
                        hx-post="/recipes/form/steps"
                        hx-target="#steps-section"
                        hx-swap="outerHTML"
                        hx-include="#steps-list textarea">
                    {Html.Encode(t("recipe.add_step"))}
                </button>
            </div>
            """;
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
