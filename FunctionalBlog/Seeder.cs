namespace FunctionalBlog;

public static class Seeder
{
    private const string AdminEmail = "admin@blog.de";
    private const string AdminPassword = "Admin1234";
    private const string AdminRoleName = "Admin";
    private const string DefaultRoleName = "Benutzer";

    public static async ValueTask SeedAsync(Env env)
    {
        await SeedRoles(env);
        await SeedAdminUser(env);
        await SeedSampleArticles(env);
    }

    private static async ValueTask SeedRoles(Env env)
    {
        if (await env.Roles.FindByName(DefaultRoleName) is null)
        {
            var id = await env.Roles.NextId();
            var role = Role.Create(id, DefaultRoleName)
                .AddRule(new PermissionRule("View", "article"));
            await env.Roles.Save(role);
        }

        if (await env.Roles.FindByName(AdminRoleName) is null)
        {
            var id = await env.Roles.NextId();
            var role = Role.Create(id, AdminRoleName)
                .AddRule(new PermissionRule("Manage", "article"))
                .AddRule(new PermissionRule("Manage", "user"))
                .AddRule(new PermissionRule("Manage", "role"))
                .AddRule(new PermissionRule("Manage", "rule"))
                .AddRule(new PermissionRule("Create", "article"))
                .AddRule(new PermissionRule("Edit", "article"))
                .AddRule(new PermissionRule("Delete", "article"))
                .AddRule(new PermissionRule("Create", "role"))
                .AddRule(new PermissionRule("View", "article"));
            await env.Roles.Save(role);
        }
    }

    private static async ValueTask SeedAdminUser(Env env)
    {
        var email = Email.Parse(AdminEmail)!;

        if (await env.Users.FindByEmail(email) is not null)
        {
            return;
        }

        var id = await env.Users.NextId();
        var hash = env.PasswordHasher.Hash(AdminPassword);
        var user = User.Create(id, email, new DisplayName("Admin"), hash, [AdminRoleName], env.Clock.Now);
        await env.Users.Save(user);
    }

    private static async ValueTask SeedSampleArticles(Env env)
    {
        if ((await env.Articles.All()).Count > 0)
        {
            return;
        }

        var admin = await env.Users.FindByEmail(Email.Parse(AdminEmail)!);

        if (admin is null)
        {
            return;
        }

        var id1 = await env.Articles.NextId();
        await env.Articles.Save(Article.Create(
            id1,
            new ArticleTitle("Hallo funktionales Blog"),
            new ArticleTeaser("Ein funktionaler Ansatz für einen modernen Blog mit .NET 10."),
            new ArticleText("Dieser Blog wurde mit einem funktionalen Ansatz in .NET 10 entwickelt. " +
                "Das Kernstück ist eine curried, reader-style Pipeline ausgedrückt mit Delegates."),
            admin.Id,
            new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero)));

        var id2 = await env.Articles.NextId();
        await env.Articles.Save(Article.Create(
            id2,
            new ArticleTitle("Macarons selbst backen"),
            new ArticleTeaser("Macarons sind kleine französische Mandelbaisers mit einer Cremefüllung."),
            new ArticleText("Macarons sind kleine französische Mandelbaisers mit einer Cremefüllung. " +
                "Das Rezept ist anspruchsvoll, aber das Ergebnis ist köstlich."),
            admin.Id,
            new DateTimeOffset(2026, 2, 20, 14, 0, 0, TimeSpan.Zero)));
    }
}
