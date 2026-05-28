public static class Router
{
    public static Middleware Create() => _ => request => env =>
    {
        var app = Match(request) ?? NotFound;
        return app(request)(env);
    };

    private static App? Match(Request request) =>
        (request.Method, request.Path) switch
        {
            ("GET", "/") => BlogHandlers.Index,
            ("GET", "/articles/new") => BlogHandlers.NewArticleForm,
            ("POST", "/articles") => BlogHandlers.CreateArticle,
            _ when request.Method == "GET" && TryArticlePath(request.Path, out var id) => BlogHandlers.ShowArticle(id),
            _ => null
        };

    private static bool TryArticlePath(string path, out ArticleId id)
    {
        id = default;

        const string prefix = "/articles/";
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var raw = path[prefix.Length..];
        if (!int.TryParse(raw, out var value))
        {
            return false;
        }

        id = new ArticleId(value);
        return true;
    }

    private static readonly App NotFound = _ => _ => ValueTask.FromResult(Response.NotFound());
}
