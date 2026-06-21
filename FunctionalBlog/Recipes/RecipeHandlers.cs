using System.Globalization;

namespace FunctionalBlog.Recipes;

public static class RecipeHandlers
{
    public static App Index => _ => async env =>
    {
        var recipes = await env.Recipes.All();
        var users = await env.Users.All();
        var authorNames = users.ToDictionary(u => u.Id, u => u.DisplayName.Value);
        return Response.Html(RecipeViews.Index(recipes, authorNames, env.Ctx));
    };

    public static App ShowRecipe(RecipeId id) => _ => async env =>
        await (await env.Recipes.Find(id)).Match(
            none: () => Task.FromResult(Response.NotFound(env.Ctx)),
            some: async recipe =>
            {
                var authorName = (await env.Users.FindById(recipe.AuthorId))
                    .Select(u => u.DisplayName.Value)
                    .GetOrElse("?");
                var ingredients = (await env.Ingredients.All()).ToDictionary(i => i.Id);
                return Response.Html(RecipeViews.Show(recipe, authorName, ingredients, env.Ctx));
            });

    public static App NewRecipeForm => _ => env =>
        ValueTask.FromResult(Response.Html(RecipeViews.Form(
            errors: [],
            name: string.Empty,
            description: string.Empty,
            portions: "4",
            difficulty: "0",
            tags: string.Empty,
            hints: string.Empty,
            ingredients: [(string.Empty, string.Empty, "g")],
            steps: [string.Empty],
            ctx: env.Ctx)));

    public static App CreateRecipe => request => async env =>
    {
        var action = request.Form.GetValueOrDefault("action", string.Empty);

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
                env.Ctx));
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
                env.Ctx));
        }

        return await DecodeWithImages(request).Match(
            failure: f => Task.FromResult(Response.Html(
                RecipeViews.Form(
                    f.Error,
                    ReadName(request),
                    ReadDescription(request),
                    ReadPortions(request),
                    ReadDifficulty(request),
                    ReadTags(request),
                    ReadHints(request),
                    RecipeForm.ParseIngredients(request),
                    RecipeForm.ParseRawSteps(request),
                    env.Ctx,
                    existingImages: RecipeForm.ParseKeptImages(request)),
                400)),
            success: async s =>
            {
                var recipe = Recipe.Create(
                    id: await env.Recipes.NextId(),
                    name: s.Value.Form.Name,
                    description: s.Value.Form.Description,
                    preparationSteps: s.Value.Form.Steps,
                    authorId: ((AuthenticatedUser)env.CurrentUser).Id,
                    difficulty: s.Value.Form.Difficulty,
                    tags: s.Value.Form.Tags,
                    portions: s.Value.Form.Portions,
                    ingredients: await ResolveIngredients(env, s.Value.Form.Ingredients),
                    images: await SaveImages(env, s.Value.Images),
                    hints: s.Value.Form.Hints);

                await env.Recipes.Save(recipe);
                env.Search?.IndexRecipe(recipe);
                return Response.Redirect($"/recipes/{recipe.Id.Value}");
            });
    };

    public static App IngredientsSection => request => env =>
    {
        var action = request.Form.GetValueOrDefault("action", string.Empty);
        var ingredients = RecipeForm.ParseIngredients(request);

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

        return ValueTask.FromResult(Response.Html(RecipeViews.IngredientSection(ingredients, env.T)));
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

    public static App IngredientSearch => request => async env =>
    {
        var index = request.Form.GetValueOrDefault("index", "0");
        var query = request.Form.GetValueOrDefault($"ingredient_name_{index}", string.Empty).Trim();

        var matches = query.Length == 0
            ? []
            : (await env.Ingredients.All())
                .Where(i => i.Name.Value.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(8)
                .ToList();

        return Response.Html(RecipeViews.IngredientMatches(index, query, matches, env.T));
    };

    public static App IngredientSelect => request => env =>
    {
        var index = request.Form.GetValueOrDefault("index", "0");
        var name = request.Form.GetValueOrDefault("name", string.Empty);
        return ValueTask.FromResult(Response.Html(RecipeViews.IngredientCombobox(index, name, env.T)));
    };

    public static App DeleteRecipe(RecipeId id) => _ => async env =>
    {
        if ((await env.Recipes.Find(id)) == Option<Recipe>.None)
        {
            return Response.NotFound(env.Ctx);
        }

        await env.Recipes.Delete(id);
        env.Search?.DeleteDocument("recipe", id.Value);
        return Response.Redirect("/recipes");
    };

    public static App EditRecipeForm(RecipeId id) => _ => async env =>
        await (await env.Recipes.Find(id)).Match(
            none: () => Task.FromResult(Response.NotFound(env.Ctx)),
            some: async recipe =>
            {
                var availableIngredients = await env.Ingredients.All();
                var nameById = availableIngredients.ToDictionary(i => i.Id, i => i.Name.Value);

                var ingredients = recipe.Ingredients
                    .Select(ri => (
                        nameById.GetValueOrNone(ri.IngredientId).GetOrElse(string.Empty),
                        ri.Amount.ToString(CultureInfo.InvariantCulture),
                        ri.Unit.Abbreviation))
                    .ToList<(string Name, string Amount, string Unit)>();

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
                    ctx: env.Ctx,
                    formAction: $"/recipes/{id.Value}",
                    titleKey: "recipe.edit_title",
                    existingImages: recipe.Images));
            });

    public static App UpdateRecipe(RecipeId id) => request => async env =>
    {
        if ((await env.Recipes.Find(id)) is not [var existing])
        {
            return Response.NotFound(env.Ctx);
        }

        var action = request.Form.GetValueOrDefault("action", string.Empty);
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
                env.Ctx,
                formAction,
                "recipe.edit_title",
                RecipeForm.ParseKeptImages(request)));
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
                env.Ctx,
                formAction,
                "recipe.edit_title",
                RecipeForm.ParseKeptImages(request)));
        }

        return await DecodeWithImages(request).Match(
            failure: f => Task.FromResult(Response.Html(
                RecipeViews.Form(
                    f.Error,
                    ReadName(request),
                    ReadDescription(request),
                    ReadPortions(request),
                    ReadDifficulty(request),
                    ReadTags(request),
                    ReadHints(request),
                    RecipeForm.ParseIngredients(request),
                    RecipeForm.ParseRawSteps(request),
                    env.Ctx,
                    formAction,
                    "recipe.edit_title",
                    RecipeForm.ParseKeptImages(request)),
                400)),
            success: async s =>
            {
                var keptImages = RecipeForm.ParseKeptImages(request);
                var newImages = await SaveImages(env, s.Value.Images);

                var updated = Recipe.Create(
                    id: id,
                    name: s.Value.Form.Name,
                    description: s.Value.Form.Description,
                    preparationSteps: s.Value.Form.Steps,
                    authorId: existing.AuthorId,
                    difficulty: s.Value.Form.Difficulty,
                    tags: s.Value.Form.Tags,
                    portions: s.Value.Form.Portions,
                    ingredients: await ResolveIngredients(env, s.Value.Form.Ingredients),
                    images: [.. keptImages, .. newImages],
                    hints: s.Value.Form.Hints);

                await env.Recipes.Save(updated);
                env.Search?.IndexRecipe(updated);
                return Response.Redirect($"/recipes/{id.Value}");
            });
    };

    private sealed record RecipeWithImages(RecipeForm.Valid Form, IReadOnlyList<ImageUploadForm.Valid> Images);

    private static Validated<IReadOnlyList<string>, RecipeWithImages> DecodeWithImages(Request request)
    {
        Func<RecipeForm.Valid, IReadOnlyList<ImageUploadForm.Valid>, RecipeWithImages> combine =
            (form, images) => new RecipeWithImages(form, images);

        return combine
            .Apply(RecipeForm.Decode(request), CombineErrors)
            .Apply(ImageUploadForm.DecodeMany(request, "images"), CombineErrors);
    }

    // Resolves each typed ingredient line to an IngredientId, quick-creating a new ingredient
    // (with default nutrition) whenever the name doesn't match an existing one. Runs in the
    // handler because it needs the repository — the form decoder stays pure.
    private static async Task<IReadOnlyList<RecipeIngredient>> ResolveIngredients(
        Env env, IReadOnlyList<RecipeForm.IngredientLine> lines)
    {
        // Imported data can hold several ingredients with the same name (e.g. two "Apfel"),
        // so build the lookup tolerantly — first occurrence wins — rather than ToDictionary,
        // which would throw on a duplicate key.
        var byName = new Dictionary<string, IngredientId>(StringComparer.OrdinalIgnoreCase);
        foreach (var ing in await env.Ingredients.All())
        {
            if (!byName.ContainsKey(ing.Name.Value))
            {
                byName[ing.Name.Value] = ing.Id;
            }
        }

        var result = new List<RecipeIngredient>();
        foreach (var line in lines)
        {
            IngredientId id;
            if (byName.GetValueOrNone(line.Name) is [var existing])
            {
                id = existing;
            }
            else
            {
                var created = Ingredient.Create(
                    id: await env.Ingredients.NextId(),
                    name: new IngredientName(line.Name),
                    image: string.Empty,
                    description: string.Empty,
                    density: 1,
                    pieceCount: 0,
                    calorificValue: 0,
                    protein: 0,
                    fat: 0,
                    carbohydrates: 0,
                    sugar: 0,
                    fiber: 0);
                await env.Ingredients.Save(created);
                env.Search?.IndexIngredient(created);
                byName[line.Name] = created.Id;
                id = created.Id;
            }

            result.Add(new RecipeIngredient(id, line.Amount, line.Unit));
        }

        return result;
    }

    private static async Task<IReadOnlyList<string>> SaveImages(Env env, IReadOnlyList<ImageUploadForm.Valid> uploads)
    {
        var urls = new List<string>();
        foreach (var upload in uploads)
        {
            var image = Image.Create(
                id: await env.Images.NextId(),
                fileName: upload.FileName,
                contentType: upload.ContentType,
                data: upload.Content,
                uploadedBy: ((AuthenticatedUser)env.CurrentUser).Id,
                createdAt: env.Clock.Now);

            await env.Images.Save(image);
            urls.Add($"/images/{image.Id.Value}");
        }

        return urls;
    }

    private static IReadOnlyList<string> CombineErrors(IReadOnlyList<string> a, IReadOnlyList<string> b) => [.. a, .. b];

    private static string ReadName(Request r) => r.Form.GetValueOrDefault("name", string.Empty);

    private static string ReadDescription(Request r) => r.Form.GetValueOrDefault("description", string.Empty);

    private static string ReadPortions(Request r) => r.Form.GetValueOrDefault("portions", "4");

    private static string ReadDifficulty(Request r) => r.Form.GetValueOrDefault("difficulty", "0");

    private static string ReadTags(Request r) => r.Form.GetValueOrDefault("tags", string.Empty);

    private static string ReadHints(Request r) => r.Form.GetValueOrDefault("hints", string.Empty);
}
