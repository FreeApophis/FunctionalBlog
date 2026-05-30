namespace FunctionalBlog.Test;

public sealed class SeederTests
{
    [Fact]
    public async Task SeedAsync_creates_admin_user()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        var admin = await env.Users.FindByEmail(new Email("admin@blog.de"));
        Assert.NotNull(admin);
    }

    [Fact]
    public async Task SeedAsync_creates_admin_role_with_manage_permissions()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        var role = await env.Roles.FindByName("Admin");
        Assert.NotNull(role);
        Assert.Contains(new PermissionRule("Manage", "user"), role.Rules);
        Assert.Contains(new PermissionRule("Manage", "role"), role.Rules);
        Assert.Contains(new PermissionRule("Manage", "rule"), role.Rules);
    }

    [Fact]
    public async Task SeedAsync_creates_default_benutzer_role()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        var role = await env.Roles.FindByName("Benutzer");
        Assert.NotNull(role);
    }

    [Fact]
    public async Task SeedAsync_is_idempotent_when_called_twice()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);
        await Seeder.SeedAsync(env);

        var users = await env.Users.All();
        Assert.Single(users, u => u.Email.Value == "admin@blog.de");
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
        CurrentUser: Guest.Instance);
}
