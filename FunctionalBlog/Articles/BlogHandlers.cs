namespace FunctionalBlog.Articles;

public static class BlogHandlers
{
    public static App Index => _ => async env =>
    {
        var articles = await env.Articles.All();
        return Response.Html(BlogViews.Index(articles, env.CurrentUser));
    };

    public static App ShowArticle(ArticleId id) => _ => async env =>
    {
        var article = await env.Articles.Find(id);

        return article is null
            ? Response.NotFound()
            : Response.Html(BlogViews.Show(article, env.CurrentUser));
    };

    public static App NewArticleForm => _ => env =>
        ValueTask.FromResult(Response.Html(BlogViews.Form(Array.Empty<string>(), string.Empty, string.Empty, env.CurrentUser)));

    public static App CreateArticle => request => async env =>
    {
        var decoded = ArticleForm.Decode(request);

        if (!decoded.IsValid)
        {
            return Response.Html(BlogViews.Form(decoded.Errors, decoded.Title, decoded.Text, env.CurrentUser), 400);
        }

        var article = Article.Create(
            id: await env.Articles.NextId(),
            title: new ArticleTitle(decoded.Title),
            text: new ArticleText(decoded.Text),
            createdAt: env.Clock.Now);

        await env.Articles.Save(article);

        return Response.Redirect($"/articles/{article.Id.Value}");
    };
}
