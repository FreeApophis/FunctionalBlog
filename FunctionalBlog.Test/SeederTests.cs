namespace FunctionalBlog.Test;

public sealed class SeederTests
{
    [Fact]
    public async Task SeedAsync_creates_admin_user()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        Assert.NotEqual(Option<User>.None, await env.Users.FindByEmail(new Email("admin@blog.de")));
    }

    [Fact]
    public async Task SeedAsync_creates_admin_role_with_manage_permissions()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        var role = (await env.Roles.FindByName("Admin")).Match(none: () => default(Role), some: r => r);
        Assert.NotNull(role);
        Assert.Contains(new PermissionRule("Manage", "user"), role!.Rules);
        Assert.Contains(new PermissionRule("Manage", "role"), role.Rules);
        Assert.Contains(new PermissionRule("Manage", "rule"), role.Rules);
    }

    [Fact]
    public async Task SeedAsync_creates_default_benutzer_role()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        Assert.NotEqual(Option<Role>.None, await env.Roles.FindByName("Benutzer"));
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

    [Fact]
    public async Task SeedAsync_creates_sample_ingredients()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        var ingredients = await env.Ingredients.All();
        Assert.Contains(ingredients, i => i.Name.Value == "Mehl");
        Assert.Contains(ingredients, i => i.Name.Value == "Zucker");
        Assert.Contains(ingredients, i => i.Name.Value == "Butter");
        Assert.Contains(ingredients, i => i.Name.Value == "Eier");
        Assert.Contains(ingredients, i => i.Name.Value == "Milch");
    }

    [Fact]
    public async Task SeedAsync_creates_sample_recipes()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        var recipes = await env.Recipes.All();
        Assert.Contains(recipes, r => r.Name.Value == "Einfacher Rührkuchen");
        Assert.Contains(recipes, r => r.Name.Value == "Pfannkuchen");
    }

    [Fact]
    public async Task SeedAsync_creates_kartoffelgulasch_recipe()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        var recipes = await env.Recipes.All();
        Assert.Contains(recipes, r => r.Name.Value == "Kartoffelgulasch");
    }

    [Fact]
    public async Task SeedAsync_creates_aelpler_one_pot_recipe()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        var recipes = await env.Recipes.All();
        Assert.Contains(recipes, r => r.Name.Value == "Älpler One-Pot");
    }

    [Fact]
    public async Task SeedAsync_is_idempotent_for_recipes_when_called_twice()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);
        await Seeder.SeedAsync(env);

        var recipes = await env.Recipes.All();
        Assert.Single(recipes, r => r.Name.Value == "Pfannkuchen");
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
}
