namespace FunctionalBlog.Articles;

public static class BlogHandlers
{
    public static App Index => _ => async env =>
    {
        var articles = await env.Articles.All();
        var users = await env.Users.All();
        var authorNames = users.ToDictionary(u => u.Id, u => u.DisplayName.Value);
        return Response.Html(BlogViews.Index(articles, env.CurrentUser, authorNames, env.T));
    };

    public static App ShowArticle(ArticleId id) => _ => async env =>
    {
        var article = (await env.Articles.Find(id)).Match(none: () => default(Article), some: a => a);
        if (article is null)
        {
            return Response.NotFound();
        }

        var authorName = (await env.Users.FindById(article.AuthorId))
            .Select(u => u.DisplayName.Value)
            .GetOrElse("?");
        return Response.Html(BlogViews.Show(article, env.CurrentUser, authorName, env.T));
    };

    public static App NewArticleForm => _ => env =>
        ValueTask.FromResult(Response.Html(BlogViews.Form([], string.Empty, string.Empty, string.Empty, env.CurrentUser, env.T)));

    public static App CreateArticle => request => async env =>
    {
        var decoded = ArticleForm.Decode(request);

        if (!decoded.IsValid)
        {
            return Response.Html(BlogViews.Form(decoded.Errors, decoded.Title, decoded.Teaser, decoded.Text, env.CurrentUser, env.T), 400);
        }

        var authorId = ((AuthenticatedUser)env.CurrentUser).Id;
        var article = Article.Create(
            id: await env.Articles.NextId(),
            title: new ArticleTitle(decoded.Title),
            teaser: new ArticleTeaser(decoded.Teaser),
            text: new ArticleText(decoded.Text),
            authorId: authorId,
            publishedAt: env.Clock.Now);

        await env.Articles.Save(article);

        return Response.Redirect($"/articles/{article.Id.Value}");
    };

    public static App EditArticleForm(ArticleId id) => _ => async env =>
    {
        var article = (await env.Articles.Find(id)).Match(none: () => default(Article), some: a => a);
        if (article is null)
        {
            return Response.NotFound();
        }

        return Response.Html(BlogViews.Form(
            [],
            article.Title.Value,
            article.Teaser.Value,
            article.Text.Value,
            env.CurrentUser,
            env.T,
            formAction: $"/articles/{id.Value}",
            titleKey: "article.edit_title"));
    };

    public static App DeleteArticle(ArticleId id) => _ => async env =>
    {
        if ((await env.Articles.Find(id)) == Option<Article>.None)
        {
            return Response.NotFound();
        }

        await env.Articles.Delete(id);
        return Response.Redirect("/");
    };

    public static App UpdateArticle(ArticleId id) => request => async env =>
    {
        var existing = (await env.Articles.Find(id)).Match(none: () => default(Article), some: a => a);
        if (existing is null)
        {
            return Response.NotFound();
        }

        var decoded = ArticleForm.Decode(request);

        if (!decoded.IsValid)
        {
            return Response.Html(
                BlogViews.Form(
                    decoded.Errors,
                    decoded.Title,
                    decoded.Teaser,
                    decoded.Text,
                    env.CurrentUser,
                    env.T,
                    formAction: $"/articles/{id.Value}",
                    titleKey: "article.edit_title"),
                400);
        }

        var updated = Article.Create(
            id: id,
            title: new ArticleTitle(decoded.Title),
            teaser: new ArticleTeaser(decoded.Teaser),
            text: new ArticleText(decoded.Text),
            authorId: existing.AuthorId,
            publishedAt: existing.PublishedAt);

        await env.Articles.Save(updated);

        return Response.Redirect($"/articles/{id.Value}");
    };
}
