namespace FunctionalBlog;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var web = builder.Build();
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        App app = Functional.Compose(
            _ => _ => ValueTask.FromResult(Response.NotFound()),
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
    }

    private static Env BuildEnv()
        => new(
            Articles: new InMemoryArticleRepository(),
            Users: new InMemoryUserRepository(),
            Roles: new InMemoryRoleRepository(),
            Sessions: new InMemorySessionStore(),
            PasswordResets: new InMemoryPasswordResetTokenStore(),
            PasswordHasher: new Pbkdf2PasswordHasher(),
            Clock: new SystemClock(),
            Log: new ConsoleLog(),
            CurrentUser: Guest.Instance);
}