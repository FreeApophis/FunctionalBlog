namespace FunctionalBlog.Recipes;

public static class RecipeViews
{
    public static string Index(
        PagedResult<Recipe> page,
        IReadOnlyDictionary<UserId, string> authorNames,
        ViewContext ctx)
    {
        var (principal, t, _) = ctx;
        var recipes = page.Items;

        string Card(Recipe recipe)
        {
            var author = authorNames.TryGetValue(recipe.AuthorId, out var n) ? n : "?";
            var initial = author.Length > 0 ? author[..1].ToUpperInvariant() : "?";

            var categoryBadge = recipe.Tags.Count > 0
                ? $"""<span class="recipe-card-cat">{Html.Encode(recipe.Tags[0].Value)}</span>"""
                : string.Empty;

            var media = recipe.Images.Count > 0
                ? $"""<div class="recipe-card-media"><img src="{Html.Encode(recipe.Images[0])}" alt="{Html.Encode(recipe.Name.Value)}" />{categoryBadge}</div>"""
                : $"""<div class="recipe-card-media">{categoryBadge}</div>""";

            var diffClass = recipe.Difficulty switch
            {
                Difficulty.Hard => "diff-hard",
                Difficulty.Medium => "diff-medium",
                _ => "diff-easy",
            };

            return $"""
                <a class="recipe-card" href="/recipes/{recipe.Id.Value}">
                    {media}
                    <div class="recipe-card-body">
                        <h3>{Html.Encode(recipe.Name.Value)}</h3>
                        <div class="recipe-card-foot">
                            <span class="difficulty-pill {diffClass}">{Html.Encode(t(DifficultyKey(recipe.Difficulty)))}</span>
                            <span class="recipe-card-author"><span class="avatar">{Html.Encode(initial)}</span>{Html.Encode(author)}</span>
                        </div>
                    </div>
                </a>
                """;
        }

        var grid = recipes.Count == 0
            ? Html.P(Html.Text(t("recipe.no_recipes")))
            : Html.Raw($"""<div class="recipe-grid">{string.Concat(recipes.Select(Card))}</div>""");

        var pagination = Html.Pagination(page.CurrentPage, page.TotalPages, "/recipes", t("common.pagination"));

        var newButton = principal.Can<Create>(new RecipeResource())
            ? Html.Raw($"""<a class="btn" href="/recipes/new">{Html.Encode(t("recipe.new_recipe"))}</a>""")
            : HtmlString.Empty;

        var head = Html.Raw($"""
            <div class="page-head">
                <div class="eyebrow eyebrow-accent">{Html.Encode(t("recipe.eyebrow"))}</div>
                <div class="page-head-row"><h1>{Html.Encode(t("recipe.title"))}</h1>
            """) + newButton + Html.Raw("</div></div>");

        var body = head + grid + pagination;

        return Layout.Page(t("recipe.title"), body, ctx);
    }

    public static string Show(
        Recipe recipe,
        string authorName,
        IReadOnlyDictionary<IngredientId, Ingredient> ingredientMap,
        ViewContext ctx)
    {
        var (principal, t, csrfToken) = ctx;

        var breadcrumb = Html.Breadcrumb(
            Crumb.Link(t("recipe.title"), "/recipes"),
            Crumb.Current(recipe.Name.Value));

        var deleteButton = Html.Raw($"""<button type="submit" class="btn-secondary">{Html.Encode(t("common.delete"))}</button>""");
        var actions =
            Html.Raw("""<div class="recipe-actions">""") +
            (principal.Can<Edit>(new RecipeResource())
                ? Html.Link($"/recipes/{recipe.Id.Value}/edit", t("recipe.edit_link"))
                : HtmlString.Empty) +
            (principal.Can<Delete>(new RecipeResource())
                ? Html.Form($"/recipes/{recipe.Id.Value}/delete", Html.CsrfField(csrfToken) + deleteButton, cssClass: "inline-form")
                : HtmlString.Empty) +
            Html.Raw("</div>");

        var avatarLetter = authorName.Length > 0 ? authorName[..1].ToUpperInvariant() : "?";
        var meta = Html.Raw($"""
            <div class="recipe-meta">
                <span><span class="avatar">{Html.Encode(avatarLetter)}</span>{Html.Encode(authorName)}</span>
                <span class="dot">·</span>
                <span class="difficulty-pill">{Html.Encode(t(DifficultyKey(recipe.Difficulty)))}</span>
                <span class="dot">·</span>
                <span>{recipe.Portions} {Html.Encode(t("recipe.portions"))}</span>
            </div>
            """);

        var tags = recipe.Tags.Count > 0
            ? Html.Raw($"""<div class="tag-chips">{string.Concat(recipe.Tags.Select(tag => $"<span class=\"tag-chip\">{Html.Encode(tag.Value)}</span>"))}</div>""")
            : HtmlString.Empty;

        var images = recipe.Images.Count > 0
            ? Slider(recipe.Images, recipe.Name.Value, recipe.Id.Value)
            : HtmlString.Empty;

        var description = Html.Div("post-text", Html.Raw(BbcodeRenderer.RenderToHtml(recipe.Description.Value)));

        var ingredientItems = string.Concat(recipe.Ingredients.Select(ri =>
        {
            var name = ingredientMap.TryGetValue(ri.IngredientId, out var ing) ? Html.Encode(ing.Name.Value) : "?";
            return $"""<li><span class="amount">{ri.Amount:G29} {Html.Encode(t(ri.Unit.AbbreviationKey))}</span><span class="name">{name}</span></li>""";
        }));
        var ingredientsAside = recipe.Ingredients.Count > 0
            ? Html.Raw($"""
                <aside class="recipe-ingredients card">
                    <div class="recipe-ingredients-head"><h4>{Html.Encode(t("recipe.ingredients"))}</h4><span>{recipe.Portions} {Html.Encode(t("recipe.portions"))}</span></div>
                    <ul class="ingredient-list">{ingredientItems}</ul>
                </aside>
                """)
            : HtmlString.Empty;

        var stepCards = recipe.PreparationSteps.Count > 0
            ? string.Concat(recipe.PreparationSteps.OrderBy(s => s.SortOrder).Select((s, i) =>
                $"""<div class="step-card"><span class="step-badge">{i + 1}</span><p>{Html.Encode(s.Text)}</p></div>"""))
            : $"<p>{Html.Encode(t("recipe.no_steps"))}</p>";
        var stepsColumn = Html.Raw($"""
            <div class="recipe-steps">
                {SectionHead(t("recipe.preparation"))}
                {stepCards}
            </div>
            """);

        var bodyGrid = Html.Raw("""<div class="recipe-body">""") + ingredientsAside + stepsColumn + Html.Raw("</div>");

        var hints = recipe.Hints.Count > 0
            ? Html.Raw($"""<section class="card">{SectionHead(t("recipe.hints"))}""") +
                Html.Ul(recipe.Hints.Select(h => Html.Text(h.Text))) +
                Html.Raw("</section>")
            : HtmlString.Empty;

        var body = breadcrumb +
            actions +
            Html.H1(recipe.Name.Value) +
            meta +
            tags +
            images +
            description +
            bodyGrid +
            hints;

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
        IReadOnlyList<(string Name, string Amount, string Unit)> ingredients,
        IReadOnlyList<string> steps,
        ViewContext ctx,
        IReadOnlyList<Unit> units,
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

        HtmlString Field(string key, HtmlString control) => Html.Label(Html.Text(t(key)) + control);

        var basics =
            Html.Raw("""<section class="card">""") +
            Field("recipe.field.name", Html.Input("name", name)) +
            Field("recipe.field.description", Html.Raw($"""<textarea name="description" rows="3">{Html.Encode(description)}</textarea>""")) +
            Field("recipe.field.tags", Html.Input("tags", tags)) +
            Html.Raw("""<div class="field-grid">""") +
            Field("recipe.field.difficulty", Html.Raw($"""<select name="difficulty">{difficultyOptions}</select>""")) +
            Field("recipe.field.portions", Html.InputNumber("portions", portions, min: "1")) +
            Html.Raw("</div></section>");

        var hintsCard =
            Html.Raw($"""<section class="card">{SectionHead(t("recipe.field.hints"))}<textarea name="hints" rows="4">{Html.Encode(hints)}</textarea></section>""");

        var saveBar =
            Html.Raw("""<div class="save-bar">""") +
            Html.Button(t("recipe.submit")) +
            Html.Raw($"""<a class="btn btn-secondary" href="/recipes">{Html.Encode(t("common.cancel"))}</a>""") +
            Html.Raw("</div>");

        var formBody =
            Html.CsrfField(csrfToken) +
            Html.Raw("""<button type="submit" hidden></button>""") +
            basics +
            Html.Raw(IngredientSection(ingredients, t, units)) +
            Html.Raw(StepSection(steps, t)) +
            ImagesField(existingImages ?? [], t) +
            hintsCard +
            saveBar;
        var form = Html.Form(formAction, formBody, cssClass: "recipe-form", enctype: "multipart/form-data");

        // Edit reuses the form with a recipe-scoped formAction (/recipes/{id}); new posts to /recipes.
        var breadcrumb = titleKey == "recipe.edit_title"
            ? Html.Breadcrumb(
                Crumb.Link(t("recipe.title"), "/recipes"),
                Crumb.Link(name, formAction),
                Crumb.Current(t("common.edit")))
            : Html.Breadcrumb(
                Crumb.Link(t("recipe.title"), "/recipes"),
                Crumb.Current(t("common.new")));

        var body = breadcrumb +
            Html.H1(t(titleKey)) +
            errorHtml +
            form;

        return Layout.Page(t(titleKey), body, ctx);
    }

    public static string IngredientSection(
        IReadOnlyList<(string Name, string Amount, string Unit)> ingredients,
        Translate t,
        IReadOnlyList<Unit> units)
    {
        string IngredientRow(int i, string name, string amount, string unit)
        {
            var unitOptions = string.Concat(units.Select(u =>
            {
                var selected = u.Id.Value.ToString() == unit ? " selected" : string.Empty;
                return $"""<option value="{u.Id.Value}"{selected}>{Html.Encode(t(u.NameKey))}</option>""";
            }));

            return $"""
                <div class="ingredient-row" id="ingredient-row-{i}">
                    {Html.InputNumber($"ingredient_amount_{i}", amount, step: "any")}
                    <select name="ingredient_unit_{i}" class="ingredient-unit">{unitOptions}</select>
                    {IngredientCombobox(i.ToString(), name, t)}
                    <button type="submit" name="action" value="remove-ingredient-{i}"
                            class="icon-remove" title="{Html.Encode(t("recipe.remove"))}" aria-label="{Html.Encode(t("recipe.remove"))}"
                            hx-post="/recipes/form/ingredients"
                            hx-target="#ingredients-section"
                            hx-swap="outerHTML"
                            hx-include="#ingredients-list input, #ingredients-list select, input[name=_csrf]">
                        {TrashIcon}
                    </button>
                </div>
                """;
        }

        var rows = string.Concat(ingredients.Select((ing, i) => IngredientRow(i, ing.Name, ing.Amount, ing.Unit)));

        return $"""
            <section id="ingredients-section" class="card">
                {SectionHead(t("recipe.field.ingredients"))}
                <div class="ingredient-head">
                    <span>{Html.Encode(t("recipe.col.amount"))}</span>
                    <span>{Html.Encode(t("recipe.col.unit"))}</span>
                    <span>{Html.Encode(t("recipe.col.ingredient"))}</span>
                    <span></span>
                </div>
                <div id="ingredients-list">{rows}</div>
                <button type="submit" name="action" value="add-ingredient" class="btn-add"
                        hx-post="/recipes/form/ingredients"
                        hx-target="#ingredients-section"
                        hx-swap="outerHTML"
                        hx-include="#ingredients-list input, #ingredients-list select, input[name=_csrf]">
                    {PlusIcon}{Html.Encode(t("recipe.add_ingredient"))}
                </button>
            </section>
            """;
    }

    // A searchable ingredient combobox: a free-text input whose keystrokes htmx-post to the
    // search endpoint, rendering matching suggestions into the adjacent dropdown. The typed
    // text is what gets submitted — an unknown name becomes a new ingredient on save.
    public static string IngredientCombobox(string index, string name, Translate t) =>
        $$"""
            <div class="ingredient-combobox" id="ingredient-combobox-{{Html.Encode(index)}}">
                <input name="ingredient_name_{{Html.Encode(index)}}" value="{{Html.Encode(name)}}"
                       class="ingredient-name-input" autocomplete="off"
                       placeholder="{{Html.Encode(t("recipe.select_ingredient"))}}"
                       hx-post="/recipes/form/ingredient-search"
                       hx-trigger="keyup changed delay:200ms"
                       hx-target="#ing-matches-{{Html.Encode(index)}}"
                       hx-swap="innerHTML"
                       hx-vals='{"index": "{{Html.Encode(index)}}"}' />
                <div id="ing-matches-{{Html.Encode(index)}}" class="ingredient-matches"></div>
            </div>
            """;

    // Inner HTML for one combobox's dropdown: a clickable suggestion per match, or a hint that
    // the typed name will be created as a new ingredient when nothing matches.
    public static string IngredientMatches(string index, string query, IReadOnlyList<Ingredient> matches, Translate t)
    {
        if (matches.Count > 0)
        {
            return string.Concat(matches.Select(m =>
                $$"""
                    <button type="button" name="name" value="{{Html.Encode(m.Name.Value)}}"
                            class="ingredient-match"
                            hx-post="/recipes/form/ingredient-select"
                            hx-vals='{"index": "{{Html.Encode(index)}}"}'
                            hx-target="#ingredient-combobox-{{Html.Encode(index)}}"
                            hx-swap="outerHTML">{{Html.Encode(m.Name.Value)}}</button>
                    """));
        }

        return query.Length == 0
            ? string.Empty
            : $"""<div class="ingredient-create-hint">{Html.Encode(t("recipe.ingredient_will_be_created"))}: «{Html.Encode(query)}»</div>""";
    }

    public static string StepSection(IReadOnlyList<string> steps, Translate t)
    {
        string StepRow(int i, string text)
        {
            return $"""
                <div class="step-row" id="step-row-{i}">
                    <span class="step-badge">{i + 1}</span>
                    <textarea name="step_{i}" rows="3">{Html.Encode(text)}</textarea>
                    <button type="submit" name="action" value="remove-step-{i}"
                            class="icon-remove" title="{Html.Encode(t("recipe.remove"))}" aria-label="{Html.Encode(t("recipe.remove"))}"
                            hx-post="/recipes/form/steps"
                            hx-target="#steps-section"
                            hx-swap="outerHTML"
                            hx-include="#steps-list textarea, input[name=_csrf]">
                        {TrashIcon}
                    </button>
                </div>
                """;
        }

        var rows = string.Concat(steps.Select((s, i) => StepRow(i, s)));

        return $"""
            <section id="steps-section" class="card">
                {SectionHead(t("recipe.field.steps"))}
                <div id="steps-list">{rows}</div>
                <button type="submit" name="action" value="add-step" class="btn-add"
                        hx-post="/recipes/form/steps"
                        hx-target="#steps-section"
                        hx-swap="outerHTML"
                        hx-include="#steps-list textarea, input[name=_csrf]">
                    {PlusIcon}{Html.Encode(t("recipe.add_step"))}
                </button>
            </section>
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

        return Html.Raw($"""<section class="card">{SectionHead(t("recipe.field.images"))}""") +
            existingBlock + fileInput +
            Html.Raw("</section>");
    }

    // A card section header: serif title + a thin divider rule, per the design.
    private static string SectionHead(string title) =>
        $"""<div class="card-section-head"><h3>{Html.Encode(title)}</h3><span class="rule"></span></div>""";

    private static string DifficultyKey(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => "recipe.difficulty.easy",
        Difficulty.Medium => "recipe.difficulty.medium",
        Difficulty.Hard => "recipe.difficulty.hard",
        _ => difficulty.ToString(),
    };

    private const string TrashIcon =
        """<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9"><path d="M3 6h18M8 6V4a1 1 0 0 1 1-1h6a1 1 0 0 1 1 1v2m2 0v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"/></svg>""";

    private const string PlusIcon =
        """<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> """;
}
