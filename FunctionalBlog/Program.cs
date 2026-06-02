namespace FunctionalBlog;

internal class Program
{
    private const string ConnectionString = "Data Source=blog.db";

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var web = builder.Build();

        DatabaseMigrator.Migrate(ConnectionString);
        using var connection = SqliteConnectionFactory.Create(ConnectionString);

        var translations = new SqliteTranslationRepository(connection);
        var env = BuildEnv(connection, translations);

        await Seeder.SeedAsync(env);
        var translationCache = await TranslationCache.LoadAsync(translations);
        env = env with { TranslationCache = translationCache };

        App app = Functional.Compose(
            _ => _ => ValueTask.FromResult(Response.NotFound()),
            LanguageMiddleware.Create(),
            Middlewares.Recover,
            Middlewares.RequestLogging,
            AuthMiddleware.Create(),
            Router.Create(Routes.Build()));

        web.Run(async http =>
        {
            var request = await HttpAdapter.ToRequest(http);
            var response = await app(request)(env);
            await HttpAdapter.WriteResponse(http, response);
        });

        web.Run();
    }

    private static Env BuildEnv(System.Data.IDbConnection connection, ITranslationRepository translations)
        => new(
            Articles: new SqliteArticleRepository(connection),
            Users: new SqliteUserRepository(connection),
            Roles: new SqliteRoleRepository(connection),
            Sessions: new SqliteSessionStore(connection),
            PasswordResets: new SqlitePasswordResetTokenStore(connection),
            PasswordHasher: new Pbkdf2PasswordHasher(),
            Clock: new SystemClock(),
            Log: new ConsoleLog(),
            CurrentUser: Guest.Instance,
            Recipes: new SqliteRecipeRepository(connection),
            Ingredients: new SqliteIngredientRepository(connection),
            Translations: translations);
}
