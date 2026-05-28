public static class BlogViews
{
    public static string Index(IReadOnlyList<Article> articles)
    {
        var items = articles.Count == 0
            ? Html.P("Noch keine Artikel vorhanden.")
            : string.Join("", articles.Select(article =>
                Html.Article(
                    Html.H2(Html.Link($"/articles/{article.Id.Value}", article.Title.Value)) +
                    Html.Small($"Erstellt am {article.CreatedAt.LocalDateTime:g}") +
                    Html.P(Preview(article.Text.Value))
                )));

        return Layout.Page(
            "Blog",
            Html.H1("Blog") +
            Html.P(Html.Link("/articles/new", "Neuen Artikel schreiben")) +
            items
        );
    }

    public static string Show(Article article) =>
        Layout.Page(
            article.Title.Value,
            Html.P(Html.Link("/", "← Zurück")) +
            Html.H1(article.Title.Value) +
            Html.Small($"Erstellt am {article.CreatedAt.LocalDateTime:g}") +
            Html.Div("post-text", Html.Paragraphs(article.Text.Value))
        );

    public static string Form(IReadOnlyList<string> errors, string title, string text)
    {
        var errorHtml = errors.Count == 0
            ? ""
            : Html.Div("errors", Html.Ul(errors.Select(Html.Encode)));

        return Layout.Page(
            "Neuer Artikel",
            Html.P(Html.Link("/", "← Zurück")) +
            Html.H1("Neuer Artikel") +
            errorHtml +
            $"""
            <form method="post" action="/articles">
                <label>
                    Titel
                    <input name="title" value="{Html.Encode(title)}" />
                </label>

                <label>
                    Text
                    <textarea name="text" rows="10">{Html.Encode(text)}</textarea>
                </label>

                <button type="submit">Veröffentlichen</button>
            </form>
            """
        );
    }

    private static string Preview(string value) =>
        value.Length <= 160 ? value : value[..160] + "…";
}
