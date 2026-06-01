namespace FunctionalBlog.Ingredients;

public static class AdminIngredientViews
{
    public static string List(IReadOnlyList<Ingredient> ingredients, IPrincipal principal, Translate t)
    {
        string Row(Ingredient ing) =>
            $"<tr><td>{Html.Encode(ing.Name.Value)}</td><td>{Html.Encode(ing.Description)}</td>" +
            $"<td>{Html.Link($"/admin/ingredients/{ing.Id.Value}/edit", t("common.edit"))}</td></tr>";

        var table = $"""
            <table>
                <thead><tr><th>{Html.Encode(t("ingredient.field.name"))}</th><th>{Html.Encode(t("ingredient.field.description"))}</th><th></th></tr></thead>
                <tbody>{string.Concat(ingredients.Select(Row))}</tbody>
            </table>
            """;

        var body = Html.H1(t("ingredient.list_title")) +
            Html.P(Html.Link("/admin/ingredients/new", t("ingredient.new_ingredient"))) +
            Html.P(Html.Link("/admin/users", "← Admin")) +
            (ingredients.Count == 0 ? Html.P(t("ingredient.no_ingredients")) : table);

        return Layout.Page(t("ingredient.list_title"), body, principal, t);
    }

    public static string Form(
        IReadOnlyList<string> errors,
        string name,
        string description,
        string image,
        string density,
        string pieceCount,
        string calorificValue,
        string protein,
        string fat,
        string carbohydrates,
        string sugar,
        string fiber,
        IPrincipal principal,
        Translate t,
        string formAction = "/admin/ingredients",
        string titleKey = "ingredient.new_title")
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(key => t(key))));

        static string NumberField(string labelKey, string fieldName, string value, Translate translate, string step = "any", string min = "0") =>
            $"""
            <label>
                {Html.Encode(translate(labelKey))}
                <input name="{fieldName}" type="number" step="{step}" min="{min}" value="{Html.Encode(value)}" />
            </label>
            """;

        var form = $"""
            <form method="post" action="{Html.Encode(formAction)}">
                <label>
                    {Html.Encode(t("ingredient.field.name"))}
                    <input name="name" value="{Html.Encode(name)}" />
                </label>
                <label>
                    {Html.Encode(t("ingredient.field.description"))}
                    <textarea name="description" rows="2">{Html.Encode(description)}</textarea>
                </label>
                <label>
                    {Html.Encode(t("ingredient.field.image"))}
                    <input name="image" value="{Html.Encode(image)}" />
                </label>
                {NumberField("ingredient.field.density", "density", density, t, step: "0.001", min: "0.001")}
                {NumberField("ingredient.field.piece_count", "piece_count", pieceCount, t)}
                {NumberField("ingredient.field.calorific_value", "calorific_value", calorificValue, t)}
                {NumberField("ingredient.field.protein", "protein", protein, t)}
                {NumberField("ingredient.field.fat", "fat", fat, t)}
                {NumberField("ingredient.field.carbohydrates", "carbohydrates", carbohydrates, t)}
                {NumberField("ingredient.field.sugar", "sugar", sugar, t)}
                {NumberField("ingredient.field.fiber", "fiber", fiber, t)}
                <button type="submit">{Html.Encode(t("ingredient.submit"))}</button>
            </form>
            """;

        var body = Html.P(Html.Link("/admin/ingredients", t("ingredient.list_title"))) +
            Html.H1(t(titleKey)) +
            errorHtml +
            form;

        return Layout.Page(t(titleKey), body, principal, t);
    }
}
