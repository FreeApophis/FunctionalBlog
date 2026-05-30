public sealed record Response(
    int Status,
    string ContentType,
    IReadOnlyDictionary<string, string> Headers,
    string Body
)
{
    public static Response Html(string body, int status = 200) =>
        new(status, "text/html; charset=utf-8", EmptyHeaders, body);

    public static Response Text(string body, int status = 200) =>
        new(status, "text/plain; charset=utf-8", EmptyHeaders, body);

    public static Response Css(string body, int status = 200) =>
        new(status, "text/css; charset=utf-8", EmptyHeaders, body);

    public static Response Redirect(string location) =>
        new(303, "text/plain; charset=utf-8", new Dictionary<string, string>
        {
            ["Location"] = location
        }, "Redirecting...");

    public static Response NotFound() =>
        Html(Layout.Page("404", global::Html.H1("Nicht gefunden") + global::Html.P("Diese Seite existiert nicht.")), 404);

    private static readonly IReadOnlyDictionary<string, string> EmptyHeaders =
        new Dictionary<string, string>();
}
