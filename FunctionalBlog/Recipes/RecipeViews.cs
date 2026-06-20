namespace FunctionalBlog.Recipes;

public static class RecipeViews
{
    public static string Index(
        IReadOnlyList<Recipe> recipes,
        IReadOnlyDictionary<UserId, string> authorNames,
        ViewContext ctx)
    {
        var (principal, t, _) = ctx;

        HtmlString RecipeHtml(Recipe recipe)
        {
            var authorName = authorNames.TryGetValue(recipe.AuthorId, out var name) ? name : "?";
            var content = Html.H2(Html.Link($"/recipes/{recipe.Id.Value}", recipe.Name.Value)) +
                Html.Small($"{t("recipe.by")} {authorName} · {t(DifficultyKey(recipe.Difficulty))} · {recipe.Portions} {t("recipe.portions")}") +
                Html.P(Html.Text(recipe.Description.Value));
            return Html.Article(content);
        }

        var items = recipes.Count == 0
            ? Html.P(Html.Text(t("recipe.no_recipes")))
            : HtmlString.Concat(recipes.Select(RecipeHtml));

        var body = Html.H1(t("recipe.title")) +
            (principal.Can<Create>(new RecipeResource())
                ? Html.P(Html.Link("/recipes/new", t("recipe.new_recipe")))
                : HtmlString.Empty) +
            items;

        return Layout.Page(t("recipe.title"), body, ctx);
    }

    public static string Show(
        Recipe recipe,
        string authorName,
        IReadOnlyDictionary<IngredientId, Ingredient> ingredientMap,
        ViewContext ctx)
    {
        var (principal, t, csrfToken) = ctx;

        var tags = recipe.Tags.Count > 0
            ? Html.P(HtmlString.Join(", ", recipe.Tags.Select(tag => Html.Text(tag.Value))))
            : HtmlString.Empty;

        var images = recipe.Images.Count > 0
            ? Slider(recipe.Images, recipe.Name.Value, recipe.Id.Value)
            : HtmlString.Empty;

        var steps = recipe.PreparationSteps.Count > 0
            ? Html.Ol(recipe.PreparationSteps.OrderBy(s => s.SortOrder).Select(s => Html.Text(s.Text)))
            : Html.P(Html.Text(t("recipe.no_steps")));

        var ingredientRows = HtmlString.Concat(recipe.Ingredients.Select(ri =>
        {
            var name = ingredientMap.TryGetValue(ri.IngredientId, out var ing) ? Html.Text(ing.Name.Value) : Html.Raw("?");
            return Html.Tr(Html.Td(Html.Raw($"{ri.Amount:G29} ") + Html.Text(ri.Unit.Abbreviation)) + Html.Td(name));
        }));
        var ingredientTable = recipe.Ingredients.Count > 0
            ? Html.Table(Html.Tbody(ingredientRows))
            : HtmlString.Empty;

        var hints = recipe.Hints.Count > 0
            ? Html.Ul(recipe.Hints.Select(h => Html.Text(h.Text)))
            : HtmlString.Empty;

        var meta = Html.Small(
            $"{t("recipe.by")} {authorName} · " +
            $"{t(DifficultyKey(recipe.Difficulty))} · " +
            $"{recipe.Portions} {t("recipe.portions")}");

        var editLink = principal.Can<Edit>(new RecipeResource())
            ? Html.Raw(" · ") + Html.Link($"/recipes/{recipe.Id.Value}/edit", t("recipe.edit_link"))
            : HtmlString.Empty;

        var deleteForm = principal.Can<Delete>(new RecipeResource())
            ? Html.Form($"/recipes/{recipe.Id.Value}/delete", Html.CsrfField(csrfToken) + Html.Raw(" · ") + Html.Button(t("common.delete")), style: "display:inline")
            : HtmlString.Empty;

        var body = Html.P(Html.Link("/recipes", t("common.back")) + editLink + deleteForm) +
            Html.H1(recipe.Name.Value) +
            meta +
            tags +
            images +
            Html.Div("post-text", Html.Raw(BbcodeRenderer.RenderToHtml(recipe.Description.Value))) +
            Html.H2(Html.Text(t("recipe.preparation"))) +
            steps +
            Html.H2(Html.Text(t("recipe.ingredients"))) +
            ingredientTable +
            (recipe.Hints.Count > 0 ? Html.H2(Html.Text(t("recipe.hints"))) + hints : HtmlString.Empty);

        return Layout.Page(recipe.Name.Value, body, ctx);
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
        ViewContext ctx,
        string formAction = "/recipes",
        string titleKey = "recipe.new_title",
        IReadOnlyList<string>? existingImages = null)
    {
        var (_, t, csrfToken) = ctx;

        var errorHtml = errors.Count == 0
            ? HtmlString.Empty
            : Html.Div("errors", Html.Ul(errors.Select(key => Html.Text(t(key)))));

        var difficultyOptions = string.Concat(Enum.GetValues<Difficulty>().Select(d =>
        {
            var selected = ((int)d).ToString() == difficulty ? " selected" : string.Empty;
            return $"""<option value="{(int)d}"{selected}>{Html.Encode(t(DifficultyKey(d)))}</option>""";
        }));

        var formBody =
            Html.CsrfField(csrfToken) +
            Html.Raw("""<button type="submit" hidden></button>""") +
            Html.Label(Html.Text(t("recipe.field.name")) + Html.Input("name", name)) +
            Html.Label(Html.Text(t("recipe.field.description")) + Html.Raw($"""<textarea name="description" rows="3">{Html.Encode(description)}</textarea>""")) +
            Html.Label(Html.Text(t("recipe.field.portions")) + Html.InputNumber("portions", portions, min: "1")) +
            Html.Label(Html.Text(t("recipe.field.difficulty")) + Html.Raw($"""<select name="difficulty">{difficultyOptions}</select>""")) +
            Html.Label(Html.Text(t("recipe.field.tags")) + Html.Input("tags", tags)) +
            Html.Raw(IngredientSection(ingredients, availableIngredients, t)) +
            Html.Raw(StepSection(steps, t)) +
            ImagesField(existingImages ?? [], t) +
            Html.Label(Html.Text(t("recipe.field.hints")) + Html.Raw($"""<textarea name="hints" rows="4">{Html.Encode(hints)}</textarea>""")) +
            Html.Button(t("recipe.submit"));
        var form = Html.Form(formAction, formBody, enctype: "multipart/form-data");

        var body = Html.P(Html.Link("/recipes", t("common.back"))) +
            Html.H1(t(titleKey)) +
            errorHtml +
            form;

        return Layout.Page(t(titleKey), body, ctx);
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

            var unitOptions = string.Concat(FunctionalBlog.Domain.Recipes.Unit.All.Select(u =>
            {
                var selected = u.Abbreviation == unit ? " selected" : string.Empty;
                return $"""<option value="{Html.Encode(u.Abbreviation)}"{selected}>{Html.Encode(u.Name)}</option>""";
            }));

            return $"""
                <div class="ingredient-row" id="ingredient-row-{i}">
                    <select name="ingredient_id_{i}">{idOptions}</select>
                    {Html.InputNumber($"ingredient_amount_{i}", amount, step: "any")}
                    <select name="ingredient_unit_{i}">{unitOptions}</select>
                    <button type="submit" name="action" value="remove-ingredient-{i}"
                            hx-post="/recipes/form/ingredients"
                            hx-target="#ingredients-section"
                            hx-swap="outerHTML"
                            hx-include="#ingredients-list input, #ingredients-list select, input[name=_csrf]">
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
                        hx-include="#ingredients-list input, #ingredients-list select, input[name=_csrf]">
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
                    {Html.Label(Html.Raw(label) + Html.Raw($"<textarea name=\"step_{i}\" rows=\"2\">{Html.Encode(text)}</textarea>"))}
                    <button type="submit" name="action" value="remove-step-{i}"
                            hx-post="/recipes/form/steps"
                            hx-target="#steps-section"
                            hx-swap="outerHTML"
                            hx-include="#steps-list textarea, input[name=_csrf]">
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
                        hx-include="#steps-list textarea, input[name=_csrf]">
                    {Html.Encode(t("recipe.add_step"))}
                </button>
            </div>
            """;
    }

    // A CSS-only slider: a horizontal scroll-snap track plus anchor-link dots that
    // scroll each slide into view — no JavaScript involved.
    private static HtmlString Slider(IReadOnlyList<string> urls, string alt, int recipeId)
    {
        string SlideId(int i) => $"recipe-{recipeId}-slide-{i}";

        var slides = string.Concat(urls.Select((url, i) =>
            $"""<figure class="slide" id="{Html.Encode(SlideId(i))}"><img src="{Html.Encode(url)}" alt="{Html.Encode(alt)}" /></figure>"""));

        var dots = urls.Count > 1
            ? $"""<div class="slider-dots">{string.Concat(urls.Select((_, i) => $"""<a href="#{Html.Encode(SlideId(i))}" aria-label="{i + 1}"></a>"""))}</div>"""
            : string.Empty;

        return Html.Raw($"""<div class="slider"><div class="slider-track">{slides}</div>{dots}</div>""");
    }

    private static HtmlString ImagesField(IReadOnlyList<string> existingImages, Translate t)
    {
        var existing = HtmlString.Concat(existingImages.Select((url, i) =>
        {
            var removeLabel = Html.Label(Html.InputCheckbox($"remove_image_{i}", "on", false) + Html.Text(t("recipe.image_remove")));
            var item = Html.Img(url, string.Empty, cssClass: "image-edit-thumb") +
                Html.InputHidden($"existing_image_{i}", url) +
                removeLabel;
            return Html.Div("image-edit-item", item);
        }));

        var existingBlock = existingImages.Count > 0
            ? Html.Div("image-edit-grid", existing)
            : HtmlString.Empty;

        var fileInput = Html.Label(Html.Text(t("recipe.field.images")) + Html.InputFile("images", "image/*", multiple: true));

        return Html.Fieldset(t("recipe.field.images"), existingBlock + fileInput);
    }

    private static string DifficultyKey(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => "recipe.difficulty.easy",
        Difficulty.Medium => "recipe.difficulty.medium",
        Difficulty.Hard => "recipe.difficulty.hard",
        _ => difficulty.ToString(),
    };
}
