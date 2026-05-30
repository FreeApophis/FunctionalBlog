public static class Layout
{
    public static string Page(string title, string body) =>
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
            <main>{{body}}</main>
        </body>
        </html>
        """;
}
