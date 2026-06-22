using System.Globalization;

namespace FunctionalBlog.Articles;

public static class BlogViews
{
    public static string Index(
        IReadOnlyList<Article> articles,
        IReadOnlyDictionary<UserId, string> authorNames,
        IReadOnlyList<Recipe> recipes,
        ViewContext ctx)
    {
        var (principal, t, _) = ctx;

        string DateBadge(Article a)
        {
            var d = a.PublishedAt.LocalDateTime;
            var month = d.ToString("MMM", CultureInfo.InvariantCulture).ToUpperInvariant();
            return $"""<span class="date-badge"><span class="day">{d.Day}</span><span class="month">{Html.Encode(month)}</span></span>""";
        }

        string Media(Article a)
        {
            var img = a.CoverImageId.Match(
                none: () => string.Empty,
                some: id => $"""<img src="/images/{id.Value}" alt="{Html.Encode(a.Title.Value)}" />""");
            return $"""<div class="blog-media">{img}{DateBadge(a)}</div>""";
        }

        string Author(Article a)
        {
            var name = authorNames.GetValueOrNone(a.AuthorId).GetOrElse("?");
            var initial = name.Length > 0 ? name[..1].ToUpperInvariant() : "?";
            return $"""<span class="blog-author"><span class="avatar">{Html.Encode(initial)}</span>{Html.Encode(name)}</span>""";
        }

        string Featured(Article a) =>
            $"""
            <a class="blog-featured" href="/articles/{a.Id.Value}">
                {Media(a)}
                <div class="blog-featured-body">
                    <h2>{Html.Encode(a.Title.Value)}</h2>
                    <p class="blog-excerpt">{Html.Encode(a.Teaser.Value)}</p>
                    <div class="blog-foot">
                        {Author(a)}
                        <span class="blog-readmore">{Html.Encode(t("blog.read_more"))} →</span>
                    </div>
                </div>
            </a>
            """;

        string Card(Article a) =>
            $"""
            <a class="blog-card" href="/articles/{a.Id.Value}">
                {Media(a)}
                <div class="blog-card-body">
                    <h3>{Html.Encode(a.Title.Value)}</h3>
                    <p class="blog-excerpt">{Html.Encode(a.Teaser.Value)}</p>
                    <div class="blog-foot">{Author(a)}<span>{a.PublishedAt.LocalDateTime:d}</span></div>
                </div>
            </a>
            """;

        HtmlString feed;
        if (articles.Count == 0)
        {
            feed = Html.P(Html.Text(t("blog.no_articles")));
        }
        else
        {
            var rest = articles.Skip(1).ToList();
            var grid = rest.Count > 0
                ? Html.Raw($"""<div class="blog-grid">{string.Concat(rest.Select(Card))}</div>""")
                : HtmlString.Empty;
            feed = Html.Raw(Featured(articles[0])) + grid;
        }

        var newButton = principal.Can<Create>(new ArticleResource())
            ? Html.Raw($"""<a class="btn" href="/articles/new">{Html.Encode(t("blog.new_article"))}</a>""")
            : HtmlString.Empty;

        var head = Html.Raw($"""
            <div class="page-head">
                <div class="eyebrow eyebrow-accent">{Html.Encode(t("blog.eyebrow"))}</div>
                <div class="page-head-row"><h1>{Html.Encode(t("blog.title"))}</h1>
            """) + newButton + Html.Raw("</div></div>");

        var layout = Html.Raw("""<div class="blog-layout"><div class="blog-main">""") +
            feed +
            Html.Raw($"""</div><aside class="blog-sidebar">{Sidebar(recipes, t)}</aside></div>""");

        var body = head + layout;

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
            Html.Div("post-text", Html.Raw(BbcodeRenderer.RenderToHtml(article.Text.Value)));

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

    // Home sidebar backed by real data: the newest recipes and the most common recipe tags
    // (tags link into search, since there is no dedicated tag-filter page).
    private static string Sidebar(IReadOnlyList<Recipe> recipes, Translate t)
    {
        var recent = recipes
            .OrderByDescending(r => r.Id.Value)
            .Take(5)
            .Select((r, i) => $"""<li><a href="/recipes/{r.Id.Value}"><span class="num">{i + 1:D2}</span><span>{Html.Encode(r.Name.Value)}</span></a></li>""")
            .ToList();

        var recentCard = recent.Count > 0
            ? $"""<section class="sidebar-card"><h4 class="sidebar-head">{Html.Encode(t("blog.sidebar.recent_recipes"))}</h4><ul class="sidebar-recipes">{string.Concat(recent)}</ul></section>"""
            : string.Empty;

        var tags = recipes
            .SelectMany(r => r.Tags)
            .GroupBy(tag => tag.Value, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Select(g => g.First().Value)
            .Take(14)
            .Select(tag => $"""<a class="sidebar-tag" href="/search?q={Uri.EscapeDataString(tag)}">{Html.Encode(tag)}</a>""")
            .ToList();

        var tagsCard = tags.Count > 0
            ? $"""<section class="sidebar-card"><h4 class="sidebar-head">{Html.Encode(t("blog.sidebar.popular_tags"))}</h4><div class="sidebar-tags">{string.Concat(tags)}</div></section>"""
            : string.Empty;

        return recentCard + tagsCard;
    }
}
