namespace FunctionalBlog.Test;

public class RouterTests
{
    [Fact]
    public async Task Get_styles_css_returns_a_css_response()
    {
        var app = Router.Create()(NotFoundTerminal);
        var env = new Env(new InMemoryArticleRepository(), new SystemClock(), new ConsoleLog());
        var request = new Request("GET", "/styles.css", Empty, Empty, Empty);

        var response = await app(request)(env);

        Assert.Equal(200, response.Status);
        Assert.StartsWith("text/css", response.ContentType);
        Assert.Contains("--bg", response.Body);
    }

    private static readonly App NotFoundTerminal = _ => _ => ValueTask.FromResult(Response.NotFound());
    private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();
}
