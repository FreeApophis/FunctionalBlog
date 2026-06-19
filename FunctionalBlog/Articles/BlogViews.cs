namespace FunctionalBlog.Articles;

public static class BlogViews
{
    public static string Index(IReadOnlyList<Article> articles, IReadOnlyDictionary<UserId, string> authorNames, ViewContext ctx)
    {
        var (principal, t, _) = ctx;

        HtmlString ArticleHtml(Article article)
        {
            var authorName = authorNames
                .GetValueOrNone(article.AuthorId)
                .GetOrElse("?");

            var content = Html.H2(Html.Link($"/articles/{article.Id.Value}", article.Title.Value)) +
                Html.Small($"{t("article.by")} {authorName} · {article.PublishedAt.LocalDateTime:d}") +
                Html.P(Html.Text(article.Teaser.Value));
            return Html.Article(content);
        }

        var items = articles.Count == 0
            ? Html.P(Html.Text(t("blog.no_articles")))
            : HtmlString.Concat(articles.Select(ArticleHtml));

        var body = Html.H1(t("blog.title")) +
            (principal.Can<Create>(new ArticleResource())
                ? Html.P(Html.Link("/articles/new", t("blog.new_article")))
                : HtmlString.Empty) +
            items;

        return Layout.Page(t("blog.title"), body, ctx);
    }

    public static string Show(Article article, string authorName, ViewContext ctx)
    {
        var (principal, t, csrfToken) = ctx;

        var editLink = principal.Can<Edit>(new ArticleResource())
            ? Html.Raw(" · ") + Html.Link($"/articles/{article.Id.Value}/edit", t("common.edit"))
            : HtmlString.Empty;

        var deleteForm = principal.Can<Delete>(new ArticleResource())
            ? Html.Form($"/articles/{article.Id.Value}/delete", Html.CsrfField(csrfToken) + Html.Raw(" · ") + Html.Button(t("common.delete")), style: "display:inline")
            : HtmlString.Empty;

        var cover = article.CoverImageId.Match(
            none: () => HtmlString.Empty,
            some: imageId => Html.Div("cover", Html.Img($"/images/{imageId.Value}", article.Title.Value, cssClass: "cover-image")));

        var body = Html.P(Html.Link("/", t("common.back")) + editLink + deleteForm) +
            Html.H1(article.Title.Value) +
            Html.Small($"{t("article.by")} {authorName} · {article.PublishedAt.LocalDateTime:g}") +
            cover +
            Html.P(Html.Text(article.Teaser.Value)) +
            Html.Div("post-text", Html.Paragraphs(article.Text.Value));

        return Layout.Page(article.Title.Value, body, ctx);
    }

    public static string Form(
        IReadOnlyList<string> errors,
        string title,
        string teaser,
        string text,
        ViewContext ctx,
        string formAction = "/articles",
        string titleKey = "article.new_title",
        Option<ImageId> currentCover = default)
    {
        var (_, t, csrfToken) = ctx;

        var errorHtml = errors.Count == 0
            ? HtmlString.Empty
            : Html.Div("errors", Html.Ul(errors.Select(key => Html.Text(t(key)))));

        var coverField = currentCover.Match(
            none: () => Html.Label(Html.Text(t("article.field.cover_image")) + Html.InputFile("cover", "image/*")),
            some: imageId =>
                Html.Div("cover", Html.Img($"/images/{imageId.Value}", title, cssClass: "cover-image")) +
                Html.Label(Html.Text(t("article.field.cover_image")) + Html.InputFile("cover", "image/*")) +
                Html.Label(Html.InputCheckbox("remove_cover", "on", false) + Html.Text(t("article.cover_remove"))));

        var formBody =
            Html.CsrfField(csrfToken) +
            Html.Label(Html.Text(t("article.field.title")) + Html.Input("title", title)) +
            Html.Label(Html.Text(t("article.field.teaser")) + Html.Raw($"""<textarea name="teaser" rows="3">{Html.Encode(teaser)}</textarea>""")) +
            Html.Label(Html.Text(t("article.field.text")) + Html.Raw($"""<textarea name="text" rows="10">{Html.Encode(text)}</textarea>""")) +
            coverField +
            Html.Button(t("article.submit"));
        var form = Html.Form(formAction, formBody, enctype: "multipart/form-data");

        var body = Html.P(Html.Link("/", t("common.back"))) +
            Html.H1(t(titleKey)) +
            errorHtml +
            form;

        return Layout.Page(t(titleKey), body, ctx);
    }
}
