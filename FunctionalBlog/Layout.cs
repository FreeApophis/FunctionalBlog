namespace FunctionalBlog;

public static class Layout
{
    public static string Page(string title, HtmlString body, ViewContext ctx)
    {
        return $$"""
        <!doctype html>
        <html lang="de" data-theme="{{Html.Encode(ctx.Theme)}}">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <title>foodblog.ch - {{Html.Encode(title)}}</title>
            <link rel="stylesheet" href="/styles.css" />
            <link rel='icon' type='image/x-icon' href='/favicon.ico'>
            <link rel='icon' type='image/png' href='/favicon.png'>
            <script src="/htmx.min.js" defer></script>
            <script src="/combobox-keys.js" defer></script>
            <script src="/confirm-delete.js" defer></script>
            <script src="/autofocus-swap.js" defer></script>
        </head>
        <body>
            {{NavViews.UtilityBar()}}
            {{NavViews.Masthead(ctx)}}
            <main>{{body.Render()}}</main>
            {{NavViews.Footer(ctx)}}
        </body>
        </html>
        """;
    }

    public static string Page(string title, HtmlString body) =>
        Page(title, body, ViewContext.ForGuest());
}
