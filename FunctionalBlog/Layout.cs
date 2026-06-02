namespace FunctionalBlog;

public static class Layout
{
    public static string Page(string title, HtmlString body, IPrincipal principal, Translate? t = null)
    {
        var translate = t ?? (key => key);
        return $$"""
        <!doctype html>
        <html lang="de">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <title>{{Html.Encode(title)}}</title>
            <link rel="stylesheet" href="/styles.css" />
            <script src="/htmx.min.js" defer></script>
        </head>
        <body>
            {{NavViews.Nav(principal, translate)}}
            <main>{{body.Render()}}</main>
        </body>
        </html>
        """;
    }

    public static string Page(string title, HtmlString body) =>
        Page(title, body, Guest.Instance);
}
