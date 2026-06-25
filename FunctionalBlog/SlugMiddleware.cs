namespace FunctionalBlog;

// Loads the slug registry once per request and threads it into Env, so the rendering layer can
// build canonical /type/{slug} URLs synchronously (ViewContext.Url / SlugIndex). When no slug
// repository is configured it does nothing and URLs fall back to numeric ids.
public static class SlugMiddleware
{
    private static readonly string[] Types =
    [
        SlugEntityType.Article,
        SlugEntityType.Recipe,
        SlugEntityType.Page,
        SlugEntityType.Ingredient,
    ];

    public static Middleware Create() => next => request => async env =>
    {
        if (env.Slugs is not { } slugs)
        {
            return await next(request)(env);
        }

        var byType = new Dictionary<string, IReadOnlyDictionary<int, string>>();
        foreach (var type in Types)
        {
            byType[type] = await slugs.SlugsFor(type);
        }

        return await next(request)(env with { SlugIndex = new SlugIndex(byType) });
    };
}
