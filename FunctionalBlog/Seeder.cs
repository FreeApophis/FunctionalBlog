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
        var user = User.Create(id, email, hash, [AdminRoleName], env.Clock.Now);
        await env.Users.Save(user);
    }
}
