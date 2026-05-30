namespace FunctionalBlog.Articles;

public static class BlogViews
{
    public static string Index(IReadOnlyList<Article> articles, IPrincipal principal, IReadOnlyDictionary<UserId, string> authorNames)
    {
        string ArticleHtml(Article article)
        {
            var authorName = authorNames.TryGetValue(article.AuthorId, out var name) ? name : "Unbekannt";
            var content = Html.H2(Html.Link($"/articles/{article.Id.Value}", article.Title.Value)) +
                Html.Small($"Von {authorName} · {article.PublishedAt.LocalDateTime:d}") +
                Html.P(Html.Encode(article.Teaser.Value));
            return Html.Article(content);
        }

        var items = articles.Count == 0
            ? Html.P("Noch keine Artikel vorhanden.")
            : string.Join(string.Empty, articles.Select(ArticleHtml));

        var body = Html.H1("Blog") +
            (principal.Can<Create>(new ArticleResource())
                ? Html.P(Html.Link("/articles/new", "Neuen Artikel schreiben"))
                : string.Empty) +
            items;

        return Layout.Page("Blog", body, principal);
    }

    public static string Show(Article article, IPrincipal principal, string authorName)
    {
        var body = Html.P(Html.Link("/", "← Zurück")) +
            Html.H1(article.Title.Value) +
            Html.Small($"Von {authorName} · {article.PublishedAt.LocalDateTime:g}") +
            Html.P(Html.Encode(article.Teaser.Value)) +
            Html.Div("post-text", Html.Paragraphs(article.Text.Value));

        return Layout.Page(article.Title.Value, body, principal);
    }

    public static string Form(IReadOnlyList<string> errors, string title, string teaser, string text, IPrincipal principal)
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
                    Teaser
                    <textarea name="teaser" rows="3">{Html.Encode(teaser)}</textarea>
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

        return Layout.Page("Neuer Artikel", body, principal);
    }
}
