namespace FunctionalBlog.Search;

public static class SearchHandlers
{
    public static App Search => request => env =>
    {
        var q = request.Query.GetValueOrNone("q").GetOrElse(string.Empty).Trim();

        if (string.IsNullOrEmpty(q) || env.Search is null)
        {
            return ValueTask.FromResult(Response.Html(SearchViews.Empty(env.Ctx)));
        }

        var results = env.Search.Search(q);
        var suggestions = results.Count < 3 ? env.Search.Suggestions(q) : [];

        return ValueTask.FromResult(Response.Html(
            SearchViews.Results(q, results, suggestions, env.Ctx)));
    };
}
