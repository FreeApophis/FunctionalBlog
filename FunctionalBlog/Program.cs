using System.Collections.Concurrent;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var web = builder.Build();

var env = new Env(
    Articles: new InMemoryArticleRepository(),
    Clock: new SystemClock(),
    Log: new ConsoleLog()
);

App app = Functional.Compose(
    Middleware.Recover,
    Middleware.RequestLogging,
    Router.Create()
);

web.Run(async http =>
{
    var request = await HttpAdapter.ToRequest(http);
    var response = await app(request)(env);
    await HttpAdapter.WriteResponse(http, response);
});

web.Run();

// -----------------------------------------------------------------------------
// Functional core
// -----------------------------------------------------------------------------

public delegate ValueTask<T> Effect<T>(Env env);
public delegate Effect<Response> App(Request request);
public delegate App Middleware(App next);

public static class Functional
{
    public static App Compose(params Middleware[] middlewares)
    {
        App app = _ => _ => ValueTask.FromResult(Response.NotFound());

        for (var i = middlewares.Length - 1; i >= 0; i--)
            app = middlewares[i](app);

        return app;
    }
}

public sealed record Env(
    IArticleRepository Articles,
    IClock Clock,
    ILog Log
);

public sealed record Request(
    string Method,
    string Path,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> Query,
    IReadOnlyDictionary<string, string> Form
);

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

    public static Response Redirect(string location) =>
        new(303, "text/plain; charset=utf-8", new Dictionary<string, string>
        {
            ["Location"] = location
        }, "Redirecting...");

    public static Response NotFound() =>
        Html(Layout.Page("404", Html.H1("Nicht gefunden") + Html.P("Diese Seite existiert nicht.")), 404);

    private static readonly IReadOnlyDictionary<string, string> EmptyHeaders =
        new Dictionary<string, string>();
}

// -----------------------------------------------------------------------------
// Middleware: App -> App
// -----------------------------------------------------------------------------

public static class Middlewares
{
    public static Middleware Recover => next => request => async env =>
    {
        try
        {
            return await next(request)(env);
        }
        catch (Exception ex)
        {
            env.Log.Error(ex);
            return Response.Html(
                Layout.Page("Fehler", Html.H1("Interner Fehler") + Html.P("Es ist ein unerwarteter Fehler aufgetreten.")),
                500
            );
        }
    };

    public static Middleware RequestLogging => next => request => async env =>
    {
        var started = env.Clock.Now;
        var response = await next(request)(env);
        var elapsed = env.Clock.Now - started;

        env.Log.Info($"{request.Method} {request.Path} -> {response.Status} in {elapsed.TotalMilliseconds:0.0} ms");

        return response;
    };
}

// -----------------------------------------------------------------------------
// Routing
// -----------------------------------------------------------------------------

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
            return false;

        var raw = path[prefix.Length..];
        if (!int.TryParse(raw, out var value))
            return false;

        id = new ArticleId(value);
        return true;
    }

    private static readonly App NotFound = _ => _ => ValueTask.FromResult(Response.NotFound());
}

// -----------------------------------------------------------------------------
// Blog application layer
// -----------------------------------------------------------------------------

public static class BlogHandlers
{
    public static App Index => _ => async env =>
    {
        var articles = await env.Articles.All();
        return Response.Html(BlogViews.Index(articles));
    };

    public static App ShowArticle(ArticleId id) => _ => async env =>
    {
        var article = await env.Articles.Find(id);

        return article is null
            ? Response.NotFound()
            : Response.Html(BlogViews.Show(article));
    };

    public static App NewArticleForm => _ => _ =>
        ValueTask.FromResult(Response.Html(BlogViews.Form(Array.Empty<string>(), "", "")));

    public static App CreateArticle => request => async env =>
    {
        var decoded = ArticleForm.Decode(request);

        if (!decoded.IsValid)
            return Response.Html(BlogViews.Form(decoded.Errors, decoded.Title, decoded.Text), 400);

        var article = Article.Create(
            id: await env.Articles.NextId(),
            title: new ArticleTitle(decoded.Title),
            text: new ArticleText(decoded.Text),
            createdAt: env.Clock.Now
        );

        await env.Articles.Save(article);

        return Response.Redirect($"/articles/{article.Id.Value}");
    };
}

public sealed record ArticleId(int Value);
public sealed record ArticleTitle(string Value);
public sealed record ArticleText(string Value);

public sealed record Article(
    ArticleId Id,
    ArticleTitle Title,
    ArticleText Text,
    DateTimeOffset CreatedAt
)
{
    public static Article Create(ArticleId id, ArticleTitle title, ArticleText text, DateTimeOffset createdAt) =>
        new(id, title, text, createdAt);
}

public sealed record DecodedArticleForm(
    bool IsValid,
    IReadOnlyList<string> Errors,
    string Title,
    string Text
);

public static class ArticleForm
{
    public static DecodedArticleForm Decode(Request request)
    {
        var title = request.Form.GetValueOrDefault("title", "").Trim();
        var text = request.Form.GetValueOrDefault("text", "").Trim();

        var errors = new List<string>();

        if (title.Length < 3)
            errors.Add("Der Titel muss mindestens 3 Zeichen lang sein.");

        if (text.Length < 10)
            errors.Add("Der Text muss mindestens 10 Zeichen lang sein.");

        return new DecodedArticleForm(
            IsValid: errors.Count == 0,
            Errors: errors,
            Title: title,
            Text: text
        );
    }
}

// -----------------------------------------------------------------------------
// Repository capability
// -----------------------------------------------------------------------------

public interface IArticleRepository
{
    ValueTask<IReadOnlyList<Article>> All();
    ValueTask<Article?> Find(ArticleId id);
    ValueTask<ArticleId> NextId();
    ValueTask Save(Article article);
}

public sealed class InMemoryArticleRepository : IArticleRepository
{
    private readonly ConcurrentDictionary<int, Article> _articles = new();
    private int _nextId = 2;

    public InMemoryArticleRepository()
    {
        _articles[1] = Article.Create(
            new ArticleId(1),
            new ArticleTitle("Hallo funktionales Blog"),
            new ArticleText("Dies ist der erste Artikel. Die Anwendung ist absichtlich klein, aber funktional aufgebaut."),
            DateTimeOffset.UtcNow
        );
    }

    public ValueTask<IReadOnlyList<Article>> All()
    {
        var articles = _articles.Values
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return ValueTask.FromResult<IReadOnlyList<Article>>(articles);
    }

    public ValueTask<Article?> Find(ArticleId id)
    {
        _articles.TryGetValue(id.Value, out var article);
        return ValueTask.FromResult(article);
    }

    public ValueTask<ArticleId> NextId()
    {
        var id = Interlocked.Increment(ref _nextId);
        return ValueTask.FromResult(new ArticleId(id));
    }

    public ValueTask Save(Article article)
    {
        _articles[article.Id.Value] = article;
        return ValueTask.CompletedTask;
    }
}

// -----------------------------------------------------------------------------
// Views and rendering
// -----------------------------------------------------------------------------

public static class BlogViews
{
    public static string Index(IReadOnlyList<Article> articles)
    {
        var items = articles.Count == 0
            ? Html.P("Noch keine Artikel vorhanden.")
            : string.Join("", articles.Select(article =>
                Html.Article(
                    Html.H2(Html.Link($"/articles/{article.Id.Value}", article.Title.Value)) +
                    Html.Small($"Erstellt am {article.CreatedAt.LocalDateTime:g}") +
                    Html.P(Preview(article.Text.Value))
                )));

        return Layout.Page(
            "Blog",
            Html.H1("Blog") +
            Html.P(Html.Link("/articles/new", "Neuen Artikel schreiben")) +
            items
        );
    }

    public static string Show(Article article) =>
        Layout.Page(
            article.Title.Value,
            Html.P(Html.Link("/", "← Zurück")) +
            Html.H1(article.Title.Value) +
            Html.Small($"Erstellt am {article.CreatedAt.LocalDateTime:g}") +
            Html.Div("post-text", Html.Paragraphs(article.Text.Value))
        );

    public static string Form(IReadOnlyList<string> errors, string title, string text)
    {
        var errorHtml = errors.Count == 0
            ? ""
            : Html.Div("errors", Html.Ul(errors.Select(Html.Encode)));

        return Layout.Page(
            "Neuer Artikel",
            Html.P(Html.Link("/", "← Zurück")) +
            Html.H1("Neuer Artikel") +
            errorHtml +
            $"""
            <form method="post" action="/articles">
                <label>
                    Titel
                    <input name="title" value="{Html.Encode(title)}" />
                </label>

                <label>
                    Text
                    <textarea name="text" rows="10">{Html.Encode(text)}</textarea>
                </label>

                <button type="submit">Veröffentlichen</button>
            </form>
            """
        );
    }

    private static string Preview(string value) =>
        value.Length <= 160 ? value : value[..160] + "…";
}

public static class Layout
{
    public static string Page(string title, string body) =>
        $"""
        <!doctype html>
        <html lang="de">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <title>{Html.Encode(title)}</title>
            <style>
                :root {{
                    color-scheme: light dark;
                    --bg: #f3eadc;
                    --fg: #1f2933;
                    --muted: #6b7280;
                    --card: #fffaf1;
                    --accent: #6d4aff;
                    --danger: #a3333d;
                    --line: rgba(0,0,0,.12);
                }}

                @media (prefers-color-scheme: dark) {{
                    :root {{
                        --bg: #17212b;
                        --fg: #edf2f7;
                        --muted: #a8b3c2;
                        --card: #202c38;
                        --accent: #d3a84f;
                        --danger: #ff8a8a;
                        --line: rgba(255,255,255,.15);
                    }}
                }}

                * {{ box-sizing: border-box; }}
                body {{
                    margin: 0;
                    font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
                    background: var(--bg);
                    color: var(--fg);
                    line-height: 1.6;
                }}
                main {{
                    width: min(760px, calc(100vw - 2rem));
                    margin: 3rem auto;
                }}
                article, form, .errors {{
                    background: var(--card);
                    border: 1px solid var(--line);
                    border-radius: 1rem;
                    padding: 1rem 1.25rem;
                    margin: 1rem 0;
                    box-shadow: 0 10px 30px rgba(0,0,0,.06);
                }}
                h1, h2 {{ line-height: 1.15; }}
                a {{ color: var(--accent); font-weight: 650; }}
                small {{ color: var(--muted); display: block; margin-bottom: .75rem; }}
                label {{ display: grid; gap: .35rem; margin: 1rem 0; font-weight: 650; }}
                input, textarea {{
                    width: 100%;
                    border: 1px solid var(--line);
                    border-radius: .75rem;
                    padding: .8rem;
                    font: inherit;
                    background: color-mix(in srgb, var(--card), white 8%);
                    color: var(--fg);
                }}
                button {{
                    border: 0;
                    border-radius: 999px;
                    padding: .75rem 1.2rem;
                    font: inherit;
                    font-weight: 750;
                    cursor: pointer;
                    background: var(--accent);
                    color: white;
                }}
                .errors {{ color: var(--danger); }}
                .post-text {{ white-space: normal; }}
            </style>
        </head>
        <body>
            <main>{body}</main>
        </body>
        </html>
        """;
}

public static class Html
{
    public static string Encode(string value) => WebUtility.HtmlEncode(value);
    public static string H1(string value) => $"<h1>{Encode(value)}</h1>";
    public static string H2(string value) => $"<h2>{value}</h2>";
    public static string P(string value) => $"<p>{value}</p>";
    public static string Small(string value) => $"<small>{Encode(value)}</small>";
    public static string Link(string href, string text) => $"<a href=\"{Encode(href)}\">{Encode(text)}</a>";
    public static string Article(string body) => $"<article>{body}</article>";
    public static string Div(string cssClass, string body) => $"<div class=\"{Encode(cssClass)}\">{body}</div>";
    public static string Ul(IEnumerable<string> encodedItems) =>
        "<ul>" + string.Join("", encodedItems.Select(x => $"<li>{x}</li>")) + "</ul>";

    public static string Paragraphs(string text) =>
        string.Join("", text
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => P(Encode(x))));
}

// -----------------------------------------------------------------------------
// Infrastructure
// -----------------------------------------------------------------------------

public interface IClock
{
    DateTimeOffset Now { get; }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}

public interface ILog
{
    void Info(string message);
    void Error(Exception exception);
}

public sealed class ConsoleLog : ILog
{
    public void Info(string message) => Console.WriteLine(message);
    public void Error(Exception exception) => Console.Error.WriteLine(exception);
}

public static class HttpAdapter
{
    public static async ValueTask<Request> ToRequest(HttpContext http)
    {
        var form = new Dictionary<string, string>();

        if (http.Request.HasFormContentType)
        {
            var parsed = await http.Request.ReadFormAsync();
            form = parsed.ToDictionary(x => x.Key, x => x.Value.ToString());
        }

        return new Request(
            Method: http.Request.Method.ToUpperInvariant(),
            Path: http.Request.Path.Value ?? "/",
            Headers: http.Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
            Query: http.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()),
            Form: form
        );
    }

    public static async ValueTask WriteResponse(HttpContext http, Response response)
    {
        http.Response.StatusCode = response.Status;
        http.Response.ContentType = response.ContentType;

        foreach (var (key, value) in response.Headers)
            http.Response.Headers[key] = value;

        await http.Response.WriteAsync(response.Body, Encoding.UTF8);
    }
}
