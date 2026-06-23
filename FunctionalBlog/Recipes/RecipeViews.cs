using System.Globalization;

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

        var grid = recipes.Count == 0
            ? Html.P(Html.Text(t("recipe.no_recipes")))
            : Html.Raw($"""<div class="recipe-grid">{string.Concat(recipes.Select(r => Card(r, authorNames, t)))}</div>""");

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

    // A recipe grid card: image with a category badge, name, difficulty pill and author.
    // Shared by the recipe index and the tag page.
    public static string Card(Recipe recipe, IReadOnlyDictionary<UserId, string> authorNames, Translate t)
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

    public static string Show(
        Recipe recipe,
        string authorName,
        IReadOnlyDictionary<IngredientId, Ingredient> ingredientMap,
        int displayPortions,
        ViewContext ctx)
    {
        var (principal, t, csrfToken) = ctx;

        // How much to scale ingredient amounts and calories for the chosen serving count.
        var factor = recipe.Portions > 0 ? (decimal)displayPortions / recipe.Portions : 1m;

        var breadcrumb = Html.Breadcrumb(
            Crumb.Link(t("recipe.title"), "/recipes"),
            Crumb.Current(recipe.Name.Value));

        var editButton = principal.Can<Edit>(new RecipeResource())
            ? Html.Raw($"""<a class="icon-round" href="/recipes/{recipe.Id.Value}/edit" title="{Html.Encode(t("common.edit"))}" aria-label="{Html.Encode(t("common.edit"))}">{PencilIcon}</a>""")
            : HtmlString.Empty;

        var deleteIcon = Html.Raw($"""<button type="submit" class="icon-round icon-round-danger" title="{Html.Encode(t("common.delete"))}" aria-label="{Html.Encode(t("common.delete"))}">{TrashIcon}</button>""");
        var deleteButton = principal.Can<Delete>(new RecipeResource())
            ? Html.Form($"/recipes/{recipe.Id.Value}/delete", Html.CsrfField(csrfToken) + deleteIcon, cssClass: "inline-form")
            : HtmlString.Empty;

        var metaActions = Html.Raw("""<span class="recipe-meta-actions">""") + editButton + deleteButton + Html.Raw("</span>");

        var avatarLetter = authorName.Length > 0 ? authorName[..1].ToUpperInvariant() : "?";
        var meta =
            Html.Raw($"""
                <div class="recipe-meta">
                    <span><span class="avatar">{Html.Encode(avatarLetter)}</span>{Html.Encode(authorName)}</span>
                    <span class="dot">·</span>
                    <span class="difficulty-pill">{Html.Encode(t(DifficultyKey(recipe.Difficulty)))}</span>
                    <span class="dot">·</span>
                """) +
            PortionsMenu(recipe.Id.Value, displayPortions, t) +
            metaActions +
            Html.Raw("</div>");

        var tags = recipe.Tags.Count > 0
            ? Html.Raw($"""<div class="tag-chips">{string.Concat(recipe.Tags.Select(tag => $"<a class=\"tag-chip\" href=\"/tag/{Html.Encode(Slug.From(tag.Value))}\">{Html.Encode(tag.Value)}</a>"))}</div>""")
            : HtmlString.Empty;

        var images = recipe.Images.Count > 0
            ? Slider(recipe.Images, recipe.Name.Value, recipe.Id.Value)
            : ImagePlaceholder(recipe.Name.Value);

        // Calories are stored per serving, so they stay constant regardless of the chosen portions.
        var stats = StatStrip(recipe, recipe.CalorificValue, t);

        var description = Html.Div("post-text", Html.Raw(BbcodeRenderer.RenderToHtml(recipe.Description.Value)));

        var ingredientItems = string.Concat(recipe.Ingredients.Select(ri =>
        {
            var found = ingredientMap.TryGetValue(ri.IngredientId, out var ing);
            var name = found ? Html.Encode(ing!.Name.Value) : "?";
            var kcal = found
                ? (int)Math.Round(CalorieCalculator.ForIngredient(ri.Amount * factor, ri.Unit, ing!), MidpointRounding.AwayFromZero)
                : 0;
            var kcalLabel = kcal > 0 ? $"{kcal} {Html.Encode(t("recipe.unit.kcal"))}" : "—";
            return $"""<li><span class="amount">{FormatAmount(ri.Amount * factor)} {Html.Encode(t(ri.Unit.AbbreviationKey))}</span><span class="name">{name}</span><span class="ingredient-kcal">{kcalLabel}</span></li>""";
        }));
        var ingredientsAside = recipe.Ingredients.Count > 0
            ? Html.Raw($"""
                <aside class="recipe-ingredients card">
                    <div class="recipe-ingredients-head"><h4>{Html.Encode(t("recipe.ingredients"))}</h4><span>{displayPortions} {Html.Encode(t("recipe.portions"))}</span></div>
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
                {stepCards}
            </div>
            """);

        // Full-width section title spanning the ingredients + steps grid: "Anleitung" on the left,
        // a rule, and the step count on the right.
        var instructionsHead = Html.Raw($"""
            <div class="recipe-section-head">
                <h2>{Html.Encode(t("recipe.instructions"))}</h2>
                <span class="rule"></span>
                <span class="recipe-section-meta">{recipe.PreparationSteps.Count} {Html.Encode(t("recipe.steps"))}</span>
            </div>
            """);

        var bodyGrid = Html.Raw("""<div class="recipe-body">""") + ingredientsAside + stepsColumn + Html.Raw("</div>");

        var hints = recipe.Hints.Count > 0
            ? Html.Raw($"""<section class="card">{SectionHead(t("recipe.hints"))}""") +
                Html.Ul(recipe.Hints.Select(h => Html.Text(h.Text))) +
                Html.Raw("</section>")
            : HtmlString.Empty;

        var body = breadcrumb +
            Html.H1(recipe.Name.Value) +
            meta +
            tags +
            images +
            stats +
            description +
            instructionsHead +
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
        string prepTime,
        string cookTime,
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

        // A number input with a fixed unit suffix (e.g. "min", "kcal") rendered to the right.
        HtmlString SuffixedNumber(string fieldName, string value, string suffix) =>
            Html.Raw($"""
                <div class="input-suffix">
                    {Html.InputNumber(fieldName, value, min: "0").Render()}
                    <span class="input-suffix-label">{Html.Encode(suffix)}</span>
                </div>
                """);

        var basics =
            Html.Raw("""<section class="card">""") +
            Field("recipe.field.name", Html.Input("name", name)) +
            Field("recipe.field.description", Html.Raw($"""<textarea name="description" rows="3">{Html.Encode(description)}</textarea>""")) +
            Field("recipe.field.tags", Html.Input("tags", tags)) +
            Html.Raw("""<div class="field-grid field-grid-4">""") +
            Field("recipe.preparation", SuffixedNumber("preparation_time", prepTime, t("recipe.unit.minutes"))) +
            Field("recipe.cooking", SuffixedNumber("cooking_time", cookTime, t("recipe.unit.minutes"))) +
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

    // A CSS-only slider: a horizontal scroll-snap track plus anchor-link selection dots overlaid on
    // the image (bottom-right) that scroll each slide into view — no JavaScript involved. Automatic
    // advancing is intentionally not done here: animating scroll position needs JS, so it is deferred.
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

    // Stand-in shown when a recipe has no images: the design's diagonally-striped box with a small
    // mono label naming the dish.
    private static HtmlString ImagePlaceholder(string name) =>
        Html.Raw($"""
            <div class="recipe-image-placeholder">
                <span class="recipe-image-label">[ {Html.Encode(name.ToLowerInvariant())} ]</span>
            </div>
            """);

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

    // The hero stat strip: three cards showing calories, preparation time and cooking time,
    // each an icon + a mono label + a serif value. Calories are scaled to the chosen portions.
    private static HtmlString StatStrip(Recipe recipe, int calories, Translate t)
    {
        string Card(string icon, string label, string value) =>
            $"""
            <div class="recipe-stat">
                <span class="recipe-stat-icon">{icon}</span>
                <span class="recipe-stat-text">
                    <span class="recipe-stat-label">{Html.Encode(label)}</span>
                    <span class="recipe-stat-value">{Html.Encode(value)}</span>
                </span>
            </div>
            """;

        var minutes = t("recipe.unit.minutes");

        return Html.Raw($"""
            <div class="recipe-stats">
                {Card(CaloriesIcon, t("recipe.calories"), $"{calories} {t("recipe.unit.kcal")}")}
                {Card(PrepIcon, t("recipe.preparation"), $"{recipe.PreparationTime} {minutes}")}
                {Card(CookIcon, t("recipe.cooking"), $"{recipe.CookingTime} {minutes}")}
            </div>
            """);
    }

    // A no-JS serving-size selector: a pill trigger (people icon + count) revealing a hover/focus
    // dropdown of preset links to /recipes/{id}?portions=N. The active preset is highlighted.
    private static HtmlString PortionsMenu(int recipeId, int displayPortions, Translate t)
    {
        var options = string.Concat(PortionPresets.Select(n =>
        {
            var active = n == displayPortions ? " is-active" : string.Empty;
            return $"""<a class="portions-option{active}" href="/recipes/{recipeId}?portions={n}">{n}</a>""";
        }));

        return Html.Raw($"""
            <span class="portions-menu">
                <button type="button" class="portions-trigger" aria-haspopup="true" title="{Html.Encode(t("recipe.adjust_portions"))}">
                    {PeopleIcon}<span>{displayPortions} {Html.Encode(t("recipe.portions"))}</span>{ChevronIcon}
                </button>
                <span class="portions-dropdown"><span class="portions-panel">{options}</span></span>
            </span>
            """);
    }

    // Scaled ingredient amounts can be fractional; show at most two decimals and trim trailing zeros.
    private static string FormatAmount(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero).ToString("0.##", CultureInfo.InvariantCulture);

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
        """<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg>""";

    // Calories: a flame. Preparation: a clock. Cooking: a sand-glass / hourglass.
    private const string CaloriesIcon =
        """<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"><path d="M8.5 14.5A2.5 2.5 0 0 0 11 12c0-1.38-.5-2-1-3-1.072-2.143-.224-4.054 2-6 .5 2.5 2 4.9 4 6.5 2 1.6 3 3.5 3 5.5a7 7 0 1 1-14 0c0-1.153.433-2.294 1-3a2.5 2.5 0 0 0 2.5 2.5Z"/></svg>""";

    private const string PrepIcon =
        """<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/></svg>""";

    private const string CookIcon =
        """<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"><path d="M5 22h14M5 2h14M17 22v-4.172a2 2 0 0 0-.586-1.414L12 12l-4.414 4.414A2 2 0 0 0 7 17.828V22M7 2v4.172a2 2 0 0 0 .586 1.414L12 12l4.414-4.414A2 2 0 0 0 17 6.172V2"/></svg>""";

    private const string PeopleIcon =
        """<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"/></svg>""";

    private const string PencilIcon =
        """<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M12 20h9M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4Z"/></svg>""";

    private const string ChevronIcon =
        """<svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m6 9 6 6 6-6"/></svg>""";

    // Serving sizes offered by the portions selector on the detail page. Any positive integer is
    // accepted via ?portions=N — these are just the convenient presets.
    private static readonly int[] PortionPresets = [1, 2, 3, 4, 6, 8, 12, 20, 40, 60, 100];
}
