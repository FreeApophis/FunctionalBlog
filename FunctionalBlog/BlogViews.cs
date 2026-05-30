public static class BlogViews
{
    public static string Index(IReadOnlyList<Article> articles)
    {
        static string ArticleHtml(Article article)
        {
            var content = Html.H2(Html.Link($"/articles/{article.Id.Value}", article.Title.Value)) +
                Html.Small($"Erstellt am {article.CreatedAt.LocalDateTime:g}") +
                Html.P(Preview(article.Text.Value));
            return Html.Article(content);
        }

        var items = articles.Count == 0
            ? Html.P("Noch keine Artikel vorhanden.")
            : string.Join(string.Empty, articles.Select(ArticleHtml));

        var body = Html.H1("Blog") +
            Html.P(Html.Link("/articles/new", "Neuen Artikel schreiben")) +
            items;

        return Layout.Page("Blog", body);
    }

    public static string Show(Article article)
    {
        var body = Html.P(Html.Link("/", "← Zurück")) +
            Html.H1(article.Title.Value) +
            Html.Small($"Erstellt am {article.CreatedAt.LocalDateTime:g}") +
            Html.Div("post-text", Html.Paragraphs(article.Text.Value));

        return Layout.Page(article.Title.Value, body);
    }

    public static string Form(IReadOnlyList<string> errors, string title, string text)
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(Html.Encode)));

        var form = $"""
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
            """;

        var body = Html.P(Html.Link("/", "← Zurück")) +
            Html.H1("Neuer Artikel") +
            errorHtml +
            form;

        return Layout.Page("Neuer Artikel", body);
    }

    private static string Preview(string value) =>
        value.Length <= 160 ? value : value[..160] + "…";
}
