namespace FunctionalBlog;

public static class Layout
{
    public static string Page(string title, string body, IPrincipal principal, Translate? t = null)
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
        </head>
        <body>
            {{NavViews.Nav(principal, translate)}}
            <main>{{body}}</main>
        </body>
        </html>
        """;
    }

    public static string Page(string title, string body) =>
        Page(title, body, Guest.Instance);
}
