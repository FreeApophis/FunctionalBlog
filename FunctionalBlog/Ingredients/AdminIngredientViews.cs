namespace FunctionalBlog.Ingredients;

public static class AdminIngredientViews
{
    public static string List(IReadOnlyList<Ingredient> ingredients, ViewContext ctx, string error = "")
    {
        var (_, t, csrfToken) = ctx;

        HtmlString Row(Ingredient ing)
        {
            var deleteForm = Html.Form($"/admin/ingredients/{ing.Id.Value}/delete", Html.CsrfField(csrfToken) + Html.Button(t("common.delete")), style: "display:inline");
            return Html.Tr(
                Html.Td(Html.Text(ing.Name.Value)) +
                Html.Td(Html.Text(ing.Description)) +
                Html.Td(Html.Link($"/admin/ingredients/{ing.Id.Value}/edit", t("common.edit")) + Html.Raw(" ") + deleteForm));
        }

        var errorHtml = error == "in-use"
            ? Html.Div("errors", Html.P(Html.Text(t("ingredient.error.in_use"))))
            : HtmlString.Empty;

        var table = Html.Table(
            Html.Thead(Html.Tr(
                Html.Th(t("ingredient.field.name")) +
                Html.Th(t("ingredient.field.description")) +
                Html.Th(string.Empty))) +
            Html.Tbody(HtmlString.Concat(ingredients.Select(Row))));

        var body = Html.H1(t("ingredient.list_title")) +
            Html.P(Html.Link("/admin/ingredients/new", t("ingredient.new_ingredient"))) +
            Html.P(Html.Link("/admin", t("common.back_to_admin"))) +
            errorHtml +
            (ingredients.Count == 0 ? Html.P(Html.Text(t("ingredient.no_ingredients"))) : table);

        return Layout.Page(t("ingredient.list_title"), body, ctx);
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
        ViewContext ctx,
        string formAction = "/admin/ingredients",
        string titleKey = "ingredient.new_title")
    {
        var (_, t, csrfToken) = ctx;

        var errorHtml = errors.Count == 0
            ? HtmlString.Empty
            : Html.Div("errors", Html.Ul(errors.Select(key => Html.Text(t(key)))));

        static HtmlString NumberField(string labelKey, string fieldName, string value, Translate translate, string step = "any", string min = "0") =>
            Html.Label(Html.Text(translate(labelKey)) + Html.InputNumber(fieldName, value, min: min, step: step));

        var formBody =
            Html.CsrfField(csrfToken) +
            Html.Label(Html.Text(t("ingredient.field.name")) + Html.Input("name", name)) +
            Html.Label(Html.Text(t("ingredient.field.description")) + Html.Raw($"""<textarea name="description" rows="2">{Html.Encode(description)}</textarea>""")) +
            Html.Label(Html.Text(t("ingredient.field.image")) + Html.Input("image", image)) +
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

        return Layout.Page(t(titleKey), body, ctx);
    }
}
