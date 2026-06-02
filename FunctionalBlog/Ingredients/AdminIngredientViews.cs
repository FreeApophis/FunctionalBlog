namespace FunctionalBlog.Ingredients;

public static class AdminIngredientViews
{
    public static string List(IReadOnlyList<Ingredient> ingredients, IPrincipal principal, Translate t, string error = "")
    {
        string Row(Ingredient ing)
        {
            var deleteForm = Html.Form($"/admin/ingredients/{ing.Id.Value}/delete", Html.Button(t("common.delete")), style: "display:inline");
            return Html.Tr(
                Html.Td(Html.Encode(ing.Name.Value)) +
                Html.Td(Html.Encode(ing.Description)) +
                Html.Td(Html.Link($"/admin/ingredients/{ing.Id.Value}/edit", t("common.edit")) + " " + deleteForm));
        }

        var errorHtml = error == "in-use"
            ? Html.Div("errors", Html.P(t("ingredient.error.in_use")))
            : string.Empty;

        var table = Html.Table(
            Html.Thead(Html.Tr(
                Html.Th(t("ingredient.field.name")) +
                Html.Th(t("ingredient.field.description")) +
                Html.Th(string.Empty))) +
            Html.Tbody(string.Concat(ingredients.Select(Row))));

        var body = Html.H1(t("ingredient.list_title")) +
            Html.P(Html.Link("/admin/ingredients/new", t("ingredient.new_ingredient"))) +
            Html.P(Html.Link("/admin/users", "← Admin")) +
            errorHtml +
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
            Html.Label(Html.Encode(translate(labelKey)) + Html.InputNumber(fieldName, value, min: min, step: step));

        var formBody =
            Html.Label(Html.Encode(t("ingredient.field.name")) + Html.Input("name", name)) +
            Html.Label(Html.Encode(t("ingredient.field.description")) + $"""<textarea name="description" rows="2">{Html.Encode(description)}</textarea>""") +
            Html.Label(Html.Encode(t("ingredient.field.image")) + Html.Input("image", image)) +
            NumberField("ingredient.field.density", "density", density, t, step: "0.001", min: "0.001") +
            NumberField("ingredient.field.piece_count", "piece_count", pieceCount, t) +
            NumberField("ingredient.field.calorific_value", "calorific_value", calorificValue, t) +
            NumberField("ingredient.field.protein", "protein", protein, t) +
            NumberField("ingredient.field.fat", "fat", fat, t) +
            NumberField("ingredient.field.carbohydrates", "carbohydrates", carbohydrates, t) +
            NumberField("ingredient.field.sugar", "sugar", sugar, t) +
            NumberField("ingredient.field.fiber", "fiber", fiber, t) +
            Html.Button(t("ingredient.submit"));
        var form = Html.Form(formAction, formBody);

        var body = Html.P(Html.Link("/admin/ingredients", t("ingredient.list_title"))) +
            Html.H1(t(titleKey)) +
            errorHtml +
            form;

        return Layout.Page(t(titleKey), body, principal, t);
    }
}
