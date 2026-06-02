namespace FunctionalBlog;

public sealed record Response(
    int Status,
    string ContentType,
    IReadOnlyDictionary<string, string> Headers,
    string Body)
{
    public IReadOnlyList<string> SetCookies { get; init; } = [];

    public Response WithCookie(string cookieHeader) =>
        this with { SetCookies = [..SetCookies, cookieHeader] };

    public static Response Html(string body, int status = 200) =>
        new(status, "text/html; charset=utf-8", EmptyHeaders, body);

    public static Response Text(string body, int status = 200) =>
        new(status, "text/plain; charset=utf-8", EmptyHeaders, body);

    public static Response Css(string body, int status = 200) =>
        new(status, "text/css; charset=utf-8", EmptyHeaders, body);

    public static Response Js(string body, int status = 200) =>
        new(status, "application/javascript; charset=utf-8", EmptyHeaders, body);

    public static Response Redirect(string location) =>
        new(
            303,
            "text/plain; charset=utf-8",
            new Dictionary<string, string> { ["Location"] = location },
            "Redirecting...");

    public static Response Forbidden() =>
        Html(Layout.Page("403", FunctionalBlog.Html.H1("Keine Berechtigung") + FunctionalBlog.Html.P("Sie haben keine Berechtigung für diese Seite.")), 403);

    public static Response NotFound() =>
        Html(Layout.Page("404", FunctionalBlog.Html.H1("Nicht gefunden") + FunctionalBlog.Html.P("Diese Seite existiert nicht.")), 404);

    public static Response JsonDownload(string filename, string body) =>
        new(
            200,
            "application/json; charset=utf-8",
            new Dictionary<string, string> { ["Content-Disposition"] = $"attachment; filename=\"{filename}\"" },
            body);

    private static readonly IReadOnlyDictionary<string, string> EmptyHeaders =
        new Dictionary<string, string>();
}
