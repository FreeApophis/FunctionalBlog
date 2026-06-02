namespace FunctionalBlog.Articles;

public static class BlogViews
{
    public static string Index(IReadOnlyList<Article> articles, IPrincipal principal, IReadOnlyDictionary<UserId, string> authorNames, Translate t)
    {
        string ArticleHtml(Article article)
        {
            var authorName = authorNames.TryGetValue(article.AuthorId, out var name) ? name : "?";
            var content = Html.H2(Html.Link($"/articles/{article.Id.Value}", article.Title.Value)) +
                Html.Small($"{t("article.by")} {Html.Encode(authorName)} · {article.PublishedAt.LocalDateTime:d}") +
                Html.P(Html.Encode(article.Teaser.Value));
            return Html.Article(content);
        }

        var items = articles.Count == 0
            ? Html.P(t("blog.no_articles"))
            : string.Join(string.Empty, articles.Select(ArticleHtml));

        var body = Html.H1(t("blog.title")) +
            (principal.Can<Create>(new ArticleResource())
                ? Html.P(Html.Link("/articles/new", t("blog.new_article")))
                : string.Empty) +
            items;

        return Layout.Page(t("blog.title"), body, principal, t);
    }

    public static string Show(Article article, IPrincipal principal, string authorName, Translate t)
    {
        var editLink = principal.Can<Edit>(new ArticleResource())
            ? " · " + Html.Link($"/articles/{article.Id.Value}/edit", t("common.edit"))
            : string.Empty;

        var deleteForm = principal.Can<Delete>(new ArticleResource())
            ? Html.Form($"/articles/{article.Id.Value}/delete", " · " + Html.Button(t("common.delete")), style: "display:inline")
            : string.Empty;

        var body = Html.P(Html.Link("/", t("common.back")) + editLink + deleteForm) +
            Html.H1(article.Title.Value) +
            Html.Small($"{t("article.by")} {Html.Encode(authorName)} · {article.PublishedAt.LocalDateTime:g}") +
            Html.P(Html.Encode(article.Teaser.Value)) +
            Html.Div("post-text", Html.Paragraphs(article.Text.Value));

        return Layout.Page(article.Title.Value, body, principal, t);
    }

    public static string Form(
        IReadOnlyList<string> errors,
        string title,
        string teaser,
        string text,
        IPrincipal principal,
        Translate t,
        string formAction = "/articles",
        string titleKey = "article.new_title")
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(key => t(key))));

        var formBody =
            Html.Label(Html.Encode(t("article.field.title")) + Html.Input("title", title)) +
            Html.Label(Html.Encode(t("article.field.teaser")) + $"""<textarea name="teaser" rows="3">{Html.Encode(teaser)}</textarea>""") +
            Html.Label(Html.Encode(t("article.field.text")) + $"""<textarea name="text" rows="10">{Html.Encode(text)}</textarea>""") +
            Html.Button(t("article.submit"));
        var form = Html.Form(formAction, formBody);

        var body = Html.P(Html.Link("/", t("common.back"))) +
            Html.H1(t(titleKey)) +
            errorHtml +
            form;

        return Layout.Page(t(titleKey), body, principal, t);
    }
}
