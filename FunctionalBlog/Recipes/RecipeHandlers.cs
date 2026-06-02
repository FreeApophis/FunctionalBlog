using System.Globalization;

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
        await (await env.Recipes.Find(id)).Match(
            none: () => Task.FromResult(Response.NotFound()),
            some: async recipe =>
            {
                var authorName = (await env.Users.FindById(recipe.AuthorId))
                    .Select(u => u.DisplayName.Value)
                    .GetOrElse("?");
                var ingredients = (await env.Ingredients.All()).ToDictionary(i => i.Id);
                return Response.Html(RecipeViews.Show(recipe, env.CurrentUser, authorName, ingredients, env.T));
            });

    public static App NewRecipeForm => _ => async env =>
    {
        var availableIngredients = await env.Ingredients.All();
        return Response.Html(RecipeViews.Form(
            errors: [],
            name: string.Empty,
            description: string.Empty,
            portions: "4",
            difficulty: "0",
            tags: string.Empty,
            hints: string.Empty,
            ingredients: [(string.Empty, string.Empty, "g")],
            steps: [string.Empty],
            availableIngredients: availableIngredients,
            principal: env.CurrentUser,
            t: env.T));
    };

    public static App CreateRecipe => request => async env =>
    {
        var action = request.Form.GetValueOrDefault("action", string.Empty);
        var availableIngredients = await env.Ingredients.All();

        if (action == "add-ingredient" || action.StartsWith("remove-ingredient-"))
        {
            var formIngredients = RecipeForm.ParseIngredients(request);
            var formSteps = RecipeForm.ParseRawSteps(request);

            if (action == "add-ingredient")
            {
                formIngredients.Add((string.Empty, string.Empty, "g"));
            }
            else if (int.TryParse(action["remove-ingredient-".Length..], out var removeIdx) && removeIdx < formIngredients.Count)
            {
                formIngredients.RemoveAt(removeIdx);
            }

            return Response.Html(RecipeViews.Form(
                [],
                ReadName(request),
                ReadDescription(request),
                ReadPortions(request),
                ReadDifficulty(request),
                ReadTags(request),
                ReadHints(request),
                formIngredients,
                formSteps,
                availableIngredients,
                env.CurrentUser,
                env.T));
        }

        if (action == "add-step" || action.StartsWith("remove-step-"))
        {
            var formIngredients = RecipeForm.ParseIngredients(request);
            var formSteps = RecipeForm.ParseRawSteps(request);

            if (action == "add-step")
            {
                formSteps.Add(string.Empty);
            }
            else if (int.TryParse(action["remove-step-".Length..], out var removeIdx) && removeIdx < formSteps.Count)
            {
                formSteps.RemoveAt(removeIdx);
            }

            return Response.Html(RecipeViews.Form(
                [],
                ReadName(request),
                ReadDescription(request),
                ReadPortions(request),
                ReadDifficulty(request),
                ReadTags(request),
                ReadHints(request),
                formIngredients,
                formSteps,
                availableIngredients,
                env.CurrentUser,
                env.T));
        }

        var decoded = RecipeForm.Decode(request);
        if (!decoded.IsValid)
        {
            return Response.Html(
                RecipeViews.Form(
                    decoded.Errors,
                    decoded.Name,
                    decoded.Description,
                    decoded.Portions,
                    decoded.Difficulty,
                    decoded.Tags,
                    decoded.Hints,
                    decoded.Ingredients,
                    decoded.Steps,
                    availableIngredients,
                    env.CurrentUser,
                    env.T),
                400);
        }

        var authorId = ((AuthenticatedUser)env.CurrentUser).Id;
        var difficulty = (Difficulty)int.Parse(decoded.Difficulty);

        var recipeIngredients = decoded.Ingredients
            .Where(ing => !string.IsNullOrEmpty(ing.Id))
            .Select(ing => new RecipeIngredient(
                new IngredientId(int.Parse(ing.Id)),
                decimal.Parse(ing.Amount, CultureInfo.InvariantCulture),
                RecipeForm.ParseUnit(ing.Unit).GetOrElse(WeightUnit.Gram)))
            .ToList();

        var tags = string.IsNullOrWhiteSpace(decoded.Tags)
            ? new List<RecipeTag>()
            : decoded.Tags
                .Split(',')
                .Select(tag => new RecipeTag(tag.Trim()))
                .Where(tag => tag.Value.Length > 0)
                .ToList();

        var hints = string.IsNullOrWhiteSpace(decoded.Hints)
            ? new List<RecipeHint>()
            : decoded.Hints
                .Split('\n')
                .Select(h => h.Trim())
                .Where(h => h.Length > 0)
                .Select(h => new RecipeHint(h))
                .ToList();

        var steps = decoded.Steps
            .Where(s => s.Length > 0)
            .Select((text, i) => new PreparationStep(i + 1, text))
            .ToList();

        var recipe = Recipe.Create(
            id: await env.Recipes.NextId(),
            name: new RecipeName(decoded.Name),
            description: new RecipeDescription(decoded.Description),
            preparationSteps: steps,
            authorId: authorId,
            difficulty: difficulty,
            tags: tags,
            portions: int.Parse(decoded.Portions),
            ingredients: recipeIngredients,
            images: [],
            hints: hints);

        await env.Recipes.Save(recipe);

        return Response.Redirect($"/recipes/{recipe.Id.Value}");
    };

    public static App IngredientsSection => request => async env =>
    {
        var action = request.Form.GetValueOrDefault("action", string.Empty);
        var ingredients = RecipeForm.ParseIngredients(request);
        var availableIngredients = await env.Ingredients.All();

        if (action == "add-ingredient")
        {
            ingredients.Add((string.Empty, string.Empty, "g"));
        }
        else if (action.StartsWith("remove-ingredient-") &&
                 int.TryParse(action["remove-ingredient-".Length..], out var removeIdx) &&
                 removeIdx < ingredients.Count)
        {
            ingredients.RemoveAt(removeIdx);
        }

        return Response.Html(RecipeViews.IngredientSection(ingredients, availableIngredients, env.T));
    };

    public static App StepsSection => request => env =>
    {
        var action = request.Form.GetValueOrDefault("action", string.Empty);
        var steps = RecipeForm.ParseRawSteps(request);

        if (action == "add-step")
        {
            steps.Add(string.Empty);
        }
        else if (action.StartsWith("remove-step-") &&
                 int.TryParse(action["remove-step-".Length..], out var removeIdx) &&
                 removeIdx < steps.Count)
        {
            steps.RemoveAt(removeIdx);
        }

        return ValueTask.FromResult(Response.Html(RecipeViews.StepSection(steps, env.T)));
    };

    public static App DeleteRecipe(RecipeId id) => _ => async env =>
    {
        if ((await env.Recipes.Find(id)) == Option<Recipe>.None)
        {
            return Response.NotFound();
        }

        await env.Recipes.Delete(id);
        return Response.Redirect("/recipes");
    };

    public static App EditRecipeForm(RecipeId id) => _ => async env =>
        await (await env.Recipes.Find(id)).Match(
            none: () => Task.FromResult(Response.NotFound()),
            some: async recipe =>
            {
                var availableIngredients = await env.Ingredients.All();

                var ingredients = recipe.Ingredients
                    .Select(ri => (ri.IngredientId.Value.ToString(), ri.Amount.ToString(CultureInfo.InvariantCulture), ri.Unit.Abbreviation))
                    .ToList<(string Id, string Amount, string Unit)>();

                var steps = recipe.PreparationSteps
                    .OrderBy(s => s.SortOrder)
                    .Select(s => s.Text)
                    .ToList();

                return Response.Html(RecipeViews.Form(
                    errors: [],
                    name: recipe.Name.Value,
                    description: recipe.Description.Value,
                    portions: recipe.Portions.ToString(),
                    difficulty: ((int)recipe.Difficulty).ToString(),
                    tags: string.Join(", ", recipe.Tags.Select(tag => tag.Value)),
                    hints: string.Join("\n", recipe.Hints.Select(h => h.Text)),
                    ingredients: ingredients,
                    steps: steps,
                    availableIngredients: availableIngredients,
                    principal: env.CurrentUser,
                    t: env.T,
                    formAction: $"/recipes/{id.Value}",
                    titleKey: "recipe.edit_title"));
            });

    public static App UpdateRecipe(RecipeId id) => request => async env =>
    {
        var existingOption = await env.Recipes.Find(id);
        var existing = existingOption.Match(none: () => default(Recipe), some: r => r);
        if (existing is null)
        {
            return Response.NotFound();
        }

        var action = request.Form.GetValueOrDefault("action", string.Empty);
        var availableIngredients = await env.Ingredients.All();
        var formAction = $"/recipes/{id.Value}";

        if (action == "add-ingredient" || action.StartsWith("remove-ingredient-"))
        {
            var formIngredients = RecipeForm.ParseIngredients(request);
            var formSteps = RecipeForm.ParseRawSteps(request);

            if (action == "add-ingredient")
            {
                formIngredients.Add((string.Empty, string.Empty, "g"));
            }
            else if (int.TryParse(action["remove-ingredient-".Length..], out var removeIdx) && removeIdx < formIngredients.Count)
            {
                formIngredients.RemoveAt(removeIdx);
            }

            return Response.Html(RecipeViews.Form(
                [],
                ReadName(request),
                ReadDescription(request),
                ReadPortions(request),
                ReadDifficulty(request),
                ReadTags(request),
                ReadHints(request),
                formIngredients,
                formSteps,
                availableIngredients,
                env.CurrentUser,
                env.T,
                formAction,
                "recipe.edit_title"));
        }

        if (action == "add-step" || action.StartsWith("remove-step-"))
        {
            var formIngredients = RecipeForm.ParseIngredients(request);
            var formSteps = RecipeForm.ParseRawSteps(request);

            if (action == "add-step")
            {
                formSteps.Add(string.Empty);
            }
            else if (int.TryParse(action["remove-step-".Length..], out var removeIdx) && removeIdx < formSteps.Count)
            {
                formSteps.RemoveAt(removeIdx);
            }

            return Response.Html(RecipeViews.Form(
                [],
                ReadName(request),
                ReadDescription(request),
                ReadPortions(request),
                ReadDifficulty(request),
                ReadTags(request),
                ReadHints(request),
                formIngredients,
                formSteps,
                availableIngredients,
                env.CurrentUser,
                env.T,
                formAction,
                "recipe.edit_title"));
        }

        var decoded = RecipeForm.Decode(request);
        if (!decoded.IsValid)
        {
            return Response.Html(
                RecipeViews.Form(
                    decoded.Errors,
                    decoded.Name,
                    decoded.Description,
                    decoded.Portions,
                    decoded.Difficulty,
                    decoded.Tags,
                    decoded.Hints,
                    decoded.Ingredients,
                    decoded.Steps,
                    availableIngredients,
                    env.CurrentUser,
                    env.T,
                    formAction,
                    "recipe.edit_title"),
                400);
        }

        var difficulty = (Difficulty)int.Parse(decoded.Difficulty);

        var recipeIngredients = decoded.Ingredients
            .Where(ing => !string.IsNullOrEmpty(ing.Id))
            .Select(ing => new RecipeIngredient(
                new IngredientId(int.Parse(ing.Id)),
                decimal.Parse(ing.Amount, CultureInfo.InvariantCulture),
                RecipeForm.ParseUnit(ing.Unit).GetOrElse(WeightUnit.Gram)))
            .ToList();

        var tags = string.IsNullOrWhiteSpace(decoded.Tags)
            ? new List<RecipeTag>()
            : decoded.Tags
                .Split(',')
                .Select(tag => new RecipeTag(tag.Trim()))
                .Where(tag => tag.Value.Length > 0)
                .ToList();

        var hints = string.IsNullOrWhiteSpace(decoded.Hints)
            ? new List<RecipeHint>()
            : decoded.Hints
                .Split('\n')
                .Select(h => h.Trim())
                .Where(h => h.Length > 0)
                .Select(h => new RecipeHint(h))
                .ToList();

        var steps = decoded.Steps
            .Where(s => s.Length > 0)
            .Select((text, i) => new PreparationStep(i + 1, text))
            .ToList();

        var updated = Recipe.Create(
            id: id,
            name: new RecipeName(decoded.Name),
            description: new RecipeDescription(decoded.Description),
            preparationSteps: steps,
            authorId: existing.AuthorId,
            difficulty: difficulty,
            tags: tags,
            portions: int.Parse(decoded.Portions),
            ingredients: recipeIngredients,
            images: existing.Images,
            hints: hints);

        await env.Recipes.Save(updated);

        return Response.Redirect($"/recipes/{id.Value}");
    };

    private static string ReadName(Request r) => r.Form.GetValueOrDefault("name", string.Empty);

    private static string ReadDescription(Request r) => r.Form.GetValueOrDefault("description", string.Empty);

    private static string ReadPortions(Request r) => r.Form.GetValueOrDefault("portions", "4");

    private static string ReadDifficulty(Request r) => r.Form.GetValueOrDefault("difficulty", "0");

    private static string ReadTags(Request r) => r.Form.GetValueOrDefault("tags", string.Empty);

    private static string ReadHints(Request r) => r.Form.GetValueOrDefault("hints", string.Empty);
}
