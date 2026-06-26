namespace FunctionalBlog.Test;

public sealed class SeederTests
{
    [Fact]
    public async Task SeedAsync_creates_admin_user()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        FunctionalAssert.Some(await env.Users.FindByEmail(new Email("admin@blog.de")));
    }

    [Fact]
    public async Task SeedAsync_creates_admin_role_with_manage_permissions()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        var role = FunctionalAssert.Some(await env.Roles.FindByName("Admin"));

        Assert.Contains(new PermissionRule("Manage", "user"), role!.Rules);
        Assert.Contains(new PermissionRule("Manage", "role"), role.Rules);
        Assert.Contains(new PermissionRule("Manage", "rule"), role.Rules);
    }

    [Fact]
    public async Task SeedAsync_grants_admin_image_permissions()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        var role = FunctionalAssert.Some(await env.Roles.FindByName("Admin"));

        Assert.Contains(new PermissionRule("Create", "image"), role!.Rules);
        Assert.Contains(new PermissionRule("Delete", "image"), role.Rules);
        Assert.Contains(new PermissionRule("Manage", "image"), role.Rules);
    }

    [Fact]
    public async Task SeedAsync_grants_admin_page_permissions()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        var role = FunctionalAssert.Some(await env.Roles.FindByName("Admin"));

        Assert.Contains(new PermissionRule("Create", "page"), role!.Rules);
        Assert.Contains(new PermissionRule("Edit", "page"), role.Rules);
        Assert.Contains(new PermissionRule("Delete", "page"), role.Rules);
    }

    [Fact]
    public async Task SeedAsync_adds_missing_image_permissions_to_a_preexisting_admin_role()
    {
        var env = BuildEnv();
        var id = await env.Roles.NextId();
        await env.Roles.Save(Role.Create(id, "Admin").AddRule(new PermissionRule("Manage", "user")));

        await Seeder.SeedAsync(env);

        var role = FunctionalAssert.Some(await env.Roles.FindByName("Admin"));
        Assert.Contains(new PermissionRule("Manage", "image"), role!.Rules);
        Assert.Contains(new PermissionRule("Create", "image"), role.Rules);
    }

    [Fact]
    public async Task SeedAsync_does_not_duplicate_rules_when_called_twice()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);
        await Seeder.SeedAsync(env);

        var role = FunctionalAssert.Some(await env.Roles.FindByName("Admin"));
        Assert.Equal(role!.Rules.Distinct().Count(), role.Rules.Count);
    }

    [Fact]
    public async Task SeedAsync_creates_default_benutzer_role()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        FunctionalAssert.Some(await env.Roles.FindByName("Benutzer"));
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

    [Fact]
    public async Task SeedAsync_seeds_default_configuration_keys()
    {
        var env = BuildEnv();

        await Seeder.SeedAsync(env);

        Assert.Equal(Option.Some("foodblog.ch"), await env.Configuration!.Get(ConfigurationKeys.SiteName));
        Assert.Equal(Option.Some("587"), await env.Configuration!.Get(ConfigurationKeys.SmtpPort));
        Assert.Equal(Option.Some(string.Empty), await env.Configuration!.Get(ConfigurationKeys.SmtpHost));
    }

    [Fact]
    public async Task SeedAsync_does_not_overwrite_existing_configuration()
    {
        var env = BuildEnv();
        await env.Configuration!.Set(ConfigurationKeys.SiteName, "Mein eigener Blog");

        await Seeder.SeedAsync(env);

        Assert.Equal(Option.Some("Mein eigener Blog"), await env.Configuration!.Get(ConfigurationKeys.SiteName));
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
        Ingredients: new InMemoryIngredientRepository(),
        Units: new InMemoryUnitRepository(),
        Images: new InMemoryImageRepository(),
        Pages: new InMemoryPageRepository(),
        Configuration: new InMemoryConfigurationRepository());
}
