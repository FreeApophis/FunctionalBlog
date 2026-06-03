using System.Globalization;

namespace FunctionalBlog.Ingredients;

public static class AdminIngredientHandlers
{
    public static App List => request => async env =>
    {
        var ingredients = await env.Ingredients.All();
        var error = request.Query.GetValueOrDefault("error", string.Empty);
        return Response.Html(AdminIngredientViews.List(ingredients, env.Ctx, error));
    };

    public static App Delete(IngredientId id) => _ => async env =>
    {
        var recipes = await env.Recipes.All();
        if (recipes.Any(r => r.Ingredients.Any(i => i.IngredientId.Value == id.Value)))
        {
            return Response.Redirect("/admin/ingredients?error=in-use");
        }

        if ((await env.Ingredients.Find(id)) is [var ingredient])
        {
            await env.Ingredients.Delete(id);
            env.Search?.DeleteDocument("ingredient", id.Value);
            return Response.Redirect("/admin/ingredients");
        }

        return Response.NotFound(env.Ctx);
    };

    public static App NewForm => _ => env =>
        ValueTask.FromResult(Response.Html(AdminIngredientViews.Form(
            errors: [],
            name: string.Empty,
            description: string.Empty,
            image: string.Empty,
            density: string.Empty,
            pieceCount: "0",
            calorificValue: "0",
            protein: "0",
            fat: "0",
            carbohydrates: "0",
            sugar: "0",
            fiber: "0",
            ctx: env.Ctx)));

    public static App Create => request => async env =>
        await IngredientForm.Decode(request).Match(
            failure: f => Task.FromResult(Response.Html(
                AdminIngredientViews.Form(
                    f.Error,
                    request.Form.GetValueOrNone("name").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("description").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("image").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("density").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("piece_count").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("calorific_value").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("protein").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("fat").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("carbohydrates").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("sugar").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("fiber").GetOrElse(string.Empty),
                    env.Ctx),
                400)),
            success: async s =>
            {
                var ingredient = BuildIngredient(await env.Ingredients.NextId(), s.Value);
                await env.Ingredients.Save(ingredient);
                env.Search?.IndexIngredient(ingredient);
                return Response.Redirect("/admin/ingredients");
            });

    public static App EditForm(IngredientId id) => _ => async env =>
    {
        if ((await env.Ingredients.Find(id)) is not [var ingredient])
        {
            return Response.NotFound(env.Ctx);
        }

        return Response.Html(AdminIngredientViews.Form(
            errors: [],
            name: ingredient.Name.Value,
            description: ingredient.Description,
            image: ingredient.Image,
            density: ingredient.Density.ToString(CultureInfo.InvariantCulture),
            pieceCount: ingredient.PieceCount.ToString(CultureInfo.InvariantCulture),
            calorificValue: ingredient.CalorificValue.ToString(CultureInfo.InvariantCulture),
            protein: ingredient.Protein.ToString(CultureInfo.InvariantCulture),
            fat: ingredient.Fat.ToString(CultureInfo.InvariantCulture),
            carbohydrates: ingredient.Carbohydrates.ToString(CultureInfo.InvariantCulture),
            sugar: ingredient.Sugar.ToString(CultureInfo.InvariantCulture),
            fiber: ingredient.Fiber.ToString(CultureInfo.InvariantCulture),
            ctx: env.Ctx,
            formAction: $"/admin/ingredients/{id.Value}",
            titleKey: "ingredient.edit_title"));
    };

    public static App Update(IngredientId id) => request => async env =>
    {
        if ((await env.Ingredients.Find(id)) is not [_])
        {
            return Response.NotFound(env.Ctx);
        }

        return await IngredientForm.Decode(request).Match(
            failure: f => Task.FromResult(Response.Html(
                AdminIngredientViews.Form(
                    f.Error,
                    request.Form.GetValueOrNone("name").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("description").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("image").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("density").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("piece_count").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("calorific_value").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("protein").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("fat").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("carbohydrates").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("sugar").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("fiber").GetOrElse(string.Empty),
                    env.Ctx,
                    formAction: $"/admin/ingredients/{id.Value}",
                    titleKey: "ingredient.edit_title"),
                400)),
            success: async s =>
            {
                var updated = BuildIngredient(id, s.Value);
                await env.Ingredients.Save(updated);
                env.Search?.IndexIngredient(updated);
                return Response.Redirect("/admin/ingredients");
            });
    };

    private static Ingredient BuildIngredient(IngredientId id, IngredientForm.Valid v) =>
        Ingredient.Create(
            id,
            v.Name,
            v.Image,
            v.Description,
            v.Density,
            v.PieceCount,
            v.CalorificValue,
            v.Protein,
            v.Fat,
            v.Carbohydrates,
            v.Sugar,
            v.Fiber);
}
