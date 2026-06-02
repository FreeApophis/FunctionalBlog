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
        if ((await env.Articles.Find(id)) is [var article])
        {
            var authorName = (await env.Users.FindById(article.AuthorId))
                .Select(u => u.DisplayName.Value)
                .GetOrElse("?");

            return Response.Html(BlogViews.Show(article, env.CurrentUser, authorName, env.T));
        }

        return Response.NotFound();
    };

    public static App NewArticleForm => _ => env =>
        ValueTask.FromResult(Response.Html(BlogViews.Form([], string.Empty, string.Empty, string.Empty, env.CurrentUser, env.T)));

    public static App CreateArticle => request => async env =>
        await ArticleForm.Decode(request).Match(
            failure: f => Task.FromResult(Response.Html(
                BlogViews.Form(
                    f.Error,
                    request.Form.GetValueOrNone("title").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("teaser").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("text").GetOrElse(string.Empty),
                    env.CurrentUser,
                    env.T),
                400)),
            success: async s =>
            {
                var article = Article.Create(
                    id: await env.Articles.NextId(),
                    title: s.Value.Title,
                    teaser: s.Value.Teaser,
                    text: s.Value.Text,
                    authorId: ((AuthenticatedUser)env.CurrentUser).Id,
                    publishedAt: env.Clock.Now);

                await env.Articles.Save(article);
                env.Search?.IndexArticle(article);

                return Response.Redirect($"/articles/{article.Id.Value}");
            });

    public static App EditArticleForm(ArticleId id) => _ => async env =>
    {
        if ((await env.Articles.Find(id)) is [var article])
        {
            return Response.Html(BlogViews.Form(
                [],
                article.Title.Value,
                article.Teaser.Value,
                article.Text.Value,
                env.CurrentUser,
                env.T,
                formAction: $"/articles/{id.Value}",
                titleKey: "article.edit_title"));
        }

        return Response.NotFound();
    };

    public static App DeleteArticle(ArticleId id) => _ => async env =>
    {
        if ((await env.Articles.Find(id)) is [var article])
        {
            await env.Articles.Delete(id);
            env.Search?.DeleteDocument("article", article.Id.Value);
            return Response.Redirect("/");
        }

        return Response.NotFound();
    };

    public static App UpdateArticle(ArticleId id) => request => async env =>
    {
        if ((await env.Articles.Find(id)) is not [var existing])
        {
            return Response.NotFound();
        }

        return await ArticleForm.Decode(request).Match(
            failure: f => Task.FromResult(Response.Html(
                BlogViews.Form(
                    f.Error,
                    request.Form.GetValueOrNone("title").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("teaser").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("text").GetOrElse(string.Empty),
                    env.CurrentUser,
                    env.T,
                    formAction: $"/articles/{id.Value}",
                    titleKey: "article.edit_title"),
                400)),
            success: async s =>
            {
                var updated = Article.Create(
                    id: id,
                    title: s.Value.Title,
                    teaser: s.Value.Teaser,
                    text: s.Value.Text,
                    authorId: existing.AuthorId,
                    publishedAt: existing.PublishedAt);

                await env.Articles.Save(updated);
                env.Search?.IndexArticle(updated);

                return Response.Redirect($"/articles/{id.Value}");
            });
    };
}
