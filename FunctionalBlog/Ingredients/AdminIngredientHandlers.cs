using System.Globalization;

namespace FunctionalBlog.Ingredients;

public static class AdminIngredientHandlers
{
    public static App List => _ => async env =>
    {
        var ingredients = await env.Ingredients.All();
        return Response.Html(AdminIngredientViews.List(ingredients, env.CurrentUser, env.T));
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
            principal: env.CurrentUser,
            t: env.T)));

    public static App Create => request => async env =>
    {
        var decoded = IngredientForm.Decode(request);
        if (!decoded.IsValid)
        {
            return Response.Html(
                AdminIngredientViews.Form(
                    decoded.Errors,
                    decoded.Name,
                    decoded.Description,
                    decoded.Image,
                    decoded.Density,
                    decoded.PieceCount,
                    decoded.CalorificValue,
                    decoded.Protein,
                    decoded.Fat,
                    decoded.Carbohydrates,
                    decoded.Sugar,
                    decoded.Fiber,
                    env.CurrentUser,
                    env.T),
                400);
        }

        var ingredient = BuildIngredient(await env.Ingredients.NextId(), decoded);
        await env.Ingredients.Save(ingredient);
        return Response.Redirect("/admin/ingredients");
    };

    public static App EditForm(IngredientId id) => _ => async env =>
    {
        var ingredient = await env.Ingredients.Find(id);
        if (ingredient is null)
        {
            return Response.NotFound();
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
            principal: env.CurrentUser,
            t: env.T,
            formAction: $"/admin/ingredients/{id.Value}",
            titleKey: "ingredient.edit_title"));
    };

    public static App Update(IngredientId id) => request => async env =>
    {
        var existing = await env.Ingredients.Find(id);
        if (existing is null)
        {
            return Response.NotFound();
        }

        var decoded = IngredientForm.Decode(request);
        if (!decoded.IsValid)
        {
            return Response.Html(
                AdminIngredientViews.Form(
                    decoded.Errors,
                    decoded.Name,
                    decoded.Description,
                    decoded.Image,
                    decoded.Density,
                    decoded.PieceCount,
                    decoded.CalorificValue,
                    decoded.Protein,
                    decoded.Fat,
                    decoded.Carbohydrates,
                    decoded.Sugar,
                    decoded.Fiber,
                    env.CurrentUser,
                    env.T,
                    formAction: $"/admin/ingredients/{id.Value}",
                    titleKey: "ingredient.edit_title"),
                400);
        }

        var updated = BuildIngredient(id, decoded);
        await env.Ingredients.Save(updated);
        return Response.Redirect("/admin/ingredients");
    };

    private static Ingredient BuildIngredient(IngredientId id, DecodedIngredientForm decoded) =>
        Ingredient.Create(
            id,
            new IngredientName(decoded.Name),
            decoded.Image,
            decoded.Description,
            Parse(decoded.Density),
            Parse(decoded.PieceCount),
            Parse(decoded.CalorificValue),
            Parse(decoded.Protein),
            Parse(decoded.Fat),
            Parse(decoded.Carbohydrates),
            Parse(decoded.Sugar),
            Parse(decoded.Fiber));

    private static decimal Parse(string value) =>
        decimal.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
}
