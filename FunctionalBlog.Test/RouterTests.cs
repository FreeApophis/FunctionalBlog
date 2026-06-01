namespace FunctionalBlog.Test;

public class RouterTests
{
    [Fact]
    public async Task Get_htmx_min_js_returns_a_javascript_response()
    {
        var app = Router.Create()(NotFoundTerminal);
        var env = BuildEnv();
        var request = new Request("GET", "/htmx.min.js", Empty, Empty, Empty, Empty);

        var response = await app(request)(env);

        Assert.Equal(200, response.Status);
        Assert.StartsWith("application/javascript", response.ContentType);
        Assert.Contains("htmx", response.Body);
    }

    [Fact]
    public async Task Get_styles_css_returns_a_css_response()
    {
        var app = Router.Create()(NotFoundTerminal);
        var env = BuildEnv();
        var request = new Request("GET", "/styles.css", Empty, Empty, Empty, Empty);

        var response = await app(request)(env);

        Assert.Equal(200, response.Status);
        Assert.StartsWith("text/css", response.ContentType);
        Assert.Contains("--bg", response.Body);
    }

    [Fact]
    public async Task Get_articles_new_redirects_guest_to_login()
    {
        var app = Router.Create()(NotFoundTerminal);
        var env = BuildEnv();
        var request = new Request("GET", "/articles/new", Empty, Empty, Empty, Empty);

        var response = await app(request)(env);

        Assert.Equal(303, response.Status);
        Assert.Equal("/login", response.Headers["Location"]);
    }

    [Fact]
    public async Task Post_articles_redirects_guest_to_login()
    {
        var app = Router.Create()(NotFoundTerminal);
        var env = BuildEnv();
        var request = new Request("POST", "/articles", Empty, Empty, Empty, Empty);

        var response = await app(request)(env);

        Assert.Equal(303, response.Status);
        Assert.Equal("/login", response.Headers["Location"]);
    }

    [Fact]
    public async Task Get_recipes_returns_200_html_response()
    {
        var app = Router.Create()(NotFoundTerminal);
        var env = BuildEnv();
        var request = new Request("GET", "/recipes", Empty, Empty, Empty, Empty);

        var response = await app(request)(env);

        Assert.Equal(200, response.Status);
        Assert.StartsWith("text/html", response.ContentType);
    }

    [Fact]
    public async Task Get_recipe_by_id_returns_404_for_unknown_recipe()
    {
        var app = Router.Create()(NotFoundTerminal);
        var env = BuildEnv();
        var request = new Request("GET", "/recipes/987654", Empty, Empty, Empty, Empty);

        var response = await app(request)(env);

        Assert.Equal(404, response.Status);
    }

    [Fact]
    public async Task Get_recipes_new_redirects_guest_to_login()
    {
        var app = Router.Create()(NotFoundTerminal);
        var env = BuildEnv();
        var request = new Request("GET", "/recipes/new", Empty, Empty, Empty, Empty);

        var response = await app(request)(env);

        Assert.Equal(303, response.Status);
        Assert.Equal("/login", response.Headers["Location"]);
    }

    [Fact]
    public async Task Post_recipes_redirects_guest_to_login()
    {
        var app = Router.Create()(NotFoundTerminal);
        var env = BuildEnv();
        var request = new Request("POST", "/recipes", Empty, Empty, Empty, Empty);

        var response = await app(request)(env);

        Assert.Equal(303, response.Status);
        Assert.Equal("/login", response.Headers["Location"]);
    }

    [Fact]
    public async Task Post_recipes_form_ingredients_redirects_guest_to_login()
    {
        var app = Router.Create()(NotFoundTerminal);
        var env = BuildEnv();
        var request = new Request("POST", "/recipes/form/ingredients", Empty, Empty, Empty, Empty);

        var response = await app(request)(env);

        Assert.Equal(303, response.Status);
        Assert.Equal("/login", response.Headers["Location"]);
    }

    [Fact]
    public async Task Post_recipes_form_steps_redirects_guest_to_login()
    {
        var app = Router.Create()(NotFoundTerminal);
        var env = BuildEnv();
        var request = new Request("POST", "/recipes/form/steps", Empty, Empty, Empty, Empty);

        var response = await app(request)(env);

        Assert.Equal(303, response.Status);
        Assert.Equal("/login", response.Headers["Location"]);
    }

    private static Env BuildEnv() => new(
        Articles: new InMemoryArticleRepository(),
        Users: new InMemoryUserRepository(),
        Roles: new InMemoryRoleRepository(),
        Sessions: new InMemorySessionStore(),
        PasswordResets: new InMemoryPasswordResetTokenStore(),
        PasswordHasher: new Pbkdf2PasswordHasher(),
        Clock: new SystemClock(),
        Log: new ConsoleLog(),
        CurrentUser: Guest.Instance,
        Recipes: new InMemoryRecipeRepository(),
        Ingredients: new InMemoryIngredientRepository());

    private static readonly App NotFoundTerminal = _ => _ => ValueTask.FromResult(Response.NotFound());
    private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();
}
