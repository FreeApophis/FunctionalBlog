var builder = WebApplication.CreateBuilder(args);
var web = builder.Build();

var env = new Env(
    Articles: new InMemoryArticleRepository(),
    Users: new InMemoryUserRepository(),
    Roles: new InMemoryRoleRepository(),
    Sessions: new InMemorySessionStore(),
    PasswordResets: new InMemoryPasswordResetTokenStore(),
    PasswordHasher: new Pbkdf2PasswordHasher(),
    Clock: new SystemClock(),
    Log: new ConsoleLog(),
    CurrentUser: Guest.Instance);

await Seeder.SeedAsync(env);

App app = Functional.Compose(
    Middlewares.Recover,
    Middlewares.RequestLogging,
    AuthMiddleware.Create(),
    Router.Create());

web.Run(async http =>
{
    var request = await HttpAdapter.ToRequest(http);
    var response = await app(request)(env);
    await HttpAdapter.WriteResponse(http, response);
});

web.Run();
