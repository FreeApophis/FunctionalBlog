namespace FunctionalBlog;

public static class Layout
{
    public static string Page(string title, string body, IPrincipal principal) =>
        $$"""
        <!doctype html>
        <html lang="de">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <title>{{Html.Encode(title)}}</title>
            <link rel="stylesheet" href="/styles.css" />
        </head>
        <body>
            {{NavViews.Nav(principal)}}
            <main>{{body}}</main>
        </body>
        </html>
        """;

    public static string Page(string title, string body) =>
        Page(title, body, Guest.Instance);
}
