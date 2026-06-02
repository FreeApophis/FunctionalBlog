namespace FunctionalBlog.Test.Identity;

public sealed class AuthHandlerTests
{
    [Fact]
    public async Task Register_with_new_email_redirects_to_home_and_sets_session_cookie()
    {
        var env = BuildEnv();
        await SeedRoles(env);

        var response = await AuthHandlers.Register(RegisterRequest("neu@blog.de", "geheim123", "geheim123"))(env);

        Assert.Equal(303, response.Status);
        Assert.Equal("/", response.Headers["Location"]);
        Assert.NotEmpty(response.SetCookies);
    }

    [Fact]
    public async Task Register_with_existing_email_also_redirects_to_home_without_revealing_existence()
    {
        var env = BuildEnv();
        await SeedRoles(env);
        await AuthHandlers.Register(RegisterRequest("exist@blog.de", "geheim123", "geheim123"))(env);

        var response = await AuthHandlers.Register(RegisterRequest("exist@blog.de", "geheim123", "geheim123"))(env);

        Assert.Equal(303, response.Status);
        Assert.Equal("/", response.Headers["Location"]);
    }

    [Fact]
    public async Task Register_with_invalid_form_returns_400()
    {
        var env = BuildEnv();
        await SeedRoles(env);

        var response = await AuthHandlers.Register(RegisterRequest("notanemail", "kurz", "kurz"))(env);

        Assert.Equal(400, response.Status);
    }

    [Fact]
    public async Task Login_with_correct_credentials_redirects_and_sets_cookie()
    {
        var env = BuildEnv();
        await SeedRoles(env);
        await AuthHandlers.Register(RegisterRequest("login@blog.de", "geheim123", "geheim123"))(env);

        var response = await AuthHandlers.Login(LoginRequest("login@blog.de", "geheim123"))(env);

        Assert.Equal(303, response.Status);
        Assert.Equal("/", response.Headers["Location"]);
        Assert.NotEmpty(response.SetCookies);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_same_status_as_unknown_email()
    {
        var env = BuildEnv();
        await SeedRoles(env);
        await AuthHandlers.Register(RegisterRequest("known@blog.de", "geheim123", "geheim123"))(env);

        var wrongPassword = await AuthHandlers.Login(LoginRequest("known@blog.de", "falsch"))(env);
        var unknownEmail = await AuthHandlers.Login(LoginRequest("unknown@blog.de", "geheim123"))(env);

        Assert.Equal(wrongPassword.Status, unknownEmail.Status);
    }

    [Fact]
    public async Task Logout_deletes_session_and_returns_expired_cookie()
    {
        var env = BuildEnv();
        await SeedRoles(env);
        var loginResponse = await AuthHandlers.Register(RegisterRequest("logout@blog.de", "geheim123", "geheim123"))(env);
        var token = ExtractSessionToken(loginResponse);
        var authEnv = env with { CurrentUser = await AuthMiddleware.ResolvePrincipal(RequestWithCookie("session", token), env) };

        var response = await AuthHandlers.Logout(RequestWithCookie("session", token))(authEnv);

        Assert.Equal(303, response.Status);
        Assert.NotEmpty(response.SetCookies);
        Assert.Contains("Max-Age=0", response.SetCookies[0]);
        Assert.Null(await env.Sessions.Find(token));
    }

    [Fact]
    public async Task Password_reset_request_always_returns_200()
    {
        var env = BuildEnv();

        var knownResponse = await AuthHandlers.RequestPasswordReset(ResetRequestRequest("known@blog.de"))(env);
        var unknownResponse = await AuthHandlers.RequestPasswordReset(ResetRequestRequest("unknown@blog.de"))(env);

        Assert.Equal(200, knownResponse.Status);
        Assert.Equal(200, unknownResponse.Status);
    }

    [Fact]
    public async Task Confirm_reset_with_valid_token_updates_password_and_redirects_to_login()
    {
        var log = new TestLog();
        var env = BuildEnv(log);
        await SeedRoles(env);
        await AuthHandlers.Register(RegisterRequest("reset@blog.de", "altesPasswort1", "altesPasswort1"))(env);
        await AuthHandlers.RequestPasswordReset(ResetRequestRequest("reset@blog.de"))(env);
        var token = log.ExtractResetToken();

        var response = await AuthHandlers.ConfirmPasswordReset(
            ResetConfirmRequest(token, "neuesPasswort1", "neuesPasswort1"))(env);

        Assert.Equal(303, response.Status);
        Assert.Equal("/login", response.Headers["Location"]);

        var loginAfterReset = await AuthHandlers.Login(LoginRequest("reset@blog.de", "neuesPasswort1"))(env);
        Assert.Equal(303, loginAfterReset.Status);
    }

    [Fact]
    public async Task Confirm_reset_with_consumed_token_returns_400()
    {
        var log = new TestLog();
        var env = BuildEnv(log);
        await SeedRoles(env);
        await AuthHandlers.Register(RegisterRequest("reset2@blog.de", "altesPasswort1", "altesPasswort1"))(env);
        await AuthHandlers.RequestPasswordReset(ResetRequestRequest("reset2@blog.de"))(env);
        var token = log.ExtractResetToken();
        await AuthHandlers.ConfirmPasswordReset(ResetConfirmRequest(token, "neues1234", "neues1234"))(env);

        var response = await AuthHandlers.ConfirmPasswordReset(
            ResetConfirmRequest(token, "nochanderes", "nochanderes"))(env);

        Assert.Equal(400, response.Status);
    }

    private static Env BuildEnv(ILog? log = null) => new(
        Articles: new InMemoryArticleRepository(),
        Users: new InMemoryUserRepository(),
        Roles: new InMemoryRoleRepository(),
        Sessions: new InMemorySessionStore(),
        PasswordResets: new InMemoryPasswordResetTokenStore(),
        PasswordHasher: new Pbkdf2PasswordHasher(),
        Clock: new SystemClock(),
        Log: log ?? new ConsoleLog(),
        CurrentUser: Guest.Instance,
        Recipes: new InMemoryRecipeRepository(),
        Ingredients: new InMemoryIngredientRepository());

    private static async Task SeedRoles(Env env)
    {
        var id = await env.Roles.NextId();
        await env.Roles.Save(Role.Create(id, "Benutzer"));
    }

    private static string ExtractSessionToken(Response response) =>
        response.SetCookies[0].Split(';')[0].Replace("session=", string.Empty);

    private static Request RequestWithCookie(string name, string value) =>
        new(HttpMethod.Get, "/", Empty, Empty, Empty, new Dictionary<string, string> { [name] = value });

    private static Request RegisterRequest(string email, string password, string confirmation) =>
        new(
            HttpMethod.Post,
            "/register",
            Empty,
            Empty,
            new Dictionary<string, string> { ["email"] = email, ["displayName"] = "Testbenutzer", ["password"] = password, ["confirmation"] = confirmation },
            Empty);

    private static Request LoginRequest(string email, string password) =>
        new(
            HttpMethod.Post,
            "/login",
            Empty,
            Empty,
            new Dictionary<string, string> { ["email"] = email, ["password"] = password },
            Empty);

    private static Request ResetRequestRequest(string email) =>
        new(
            HttpMethod.Post,
            "/password-reset",
            Empty,
            Empty,
            new Dictionary<string, string> { ["email"] = email },
            Empty);

    private static Request ResetConfirmRequest(string token, string password, string confirmation) =>
        new(
            HttpMethod.Post,
            "/password-reset/confirm",
            Empty,
            Empty,
            new Dictionary<string, string> { ["token"] = token, ["password"] = password, ["confirmation"] = confirmation },
            Empty);

    private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();
}
