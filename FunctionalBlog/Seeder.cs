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
        await SeedSampleIngredients(env);
        await SeedSampleRecipes(env);

        if (env.Translations is not null)
        {
            await TranslationSeeder.SeedAsync(env.Translations);
        }
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

    private static async ValueTask SeedSampleIngredients(Env env)
    {
        if ((await env.Ingredients.All()).Count > 0)
        {
            return;
        }

        var mehlId = await env.Ingredients.NextId();
        await env.Ingredients.Save(Ingredient.Create(
            mehlId,
            new IngredientName("Mehl"),
            string.Empty,
            "Weizenmehl Type 405",
            density: 0.6m,
            pieceCount: 0m,
            calorificValue: 364m,
            protein: 10m,
            fat: 1.2m,
            carbohydrates: 76m,
            sugar: 0.3m,
            fiber: 2.7m));

        var zuckerId = await env.Ingredients.NextId();
        await env.Ingredients.Save(Ingredient.Create(
            zuckerId,
            new IngredientName("Zucker"),
            string.Empty,
            "Weißer Haushaltszucker",
            density: 1.6m,
            pieceCount: 0m,
            calorificValue: 400m,
            protein: 0m,
            fat: 0m,
            carbohydrates: 100m,
            sugar: 100m,
            fiber: 0m));

        var butterId = await env.Ingredients.NextId();
        await env.Ingredients.Save(Ingredient.Create(
            butterId,
            new IngredientName("Butter"),
            string.Empty,
            "Süßrahmbutter",
            density: 0.96m,
            pieceCount: 0m,
            calorificValue: 740m,
            protein: 0.7m,
            fat: 83m,
            carbohydrates: 0.4m,
            sugar: 0.4m,
            fiber: 0m));

        var eierId = await env.Ingredients.NextId();
        await env.Ingredients.Save(Ingredient.Create(
            eierId,
            new IngredientName("Eier"),
            string.Empty,
            "Hühnereier (Größe M, ca. 60 g)",
            density: 1.0m,
            pieceCount: 1m,
            calorificValue: 155m,
            protein: 13m,
            fat: 11m,
            carbohydrates: 1m,
            sugar: 0.4m,
            fiber: 0m));

        var milchId = await env.Ingredients.NextId();
        await env.Ingredients.Save(Ingredient.Create(
            milchId,
            new IngredientName("Milch"),
            string.Empty,
            "Vollmilch 3,5 % Fett",
            density: 1.03m,
            pieceCount: 0m,
            calorificValue: 61m,
            protein: 3.4m,
            fat: 3.4m,
            carbohydrates: 4.7m,
            sugar: 4.7m,
            fiber: 0m));
    }

    private static async ValueTask SeedSampleRecipes(Env env)
    {
        if ((await env.Recipes.All()).Count > 0)
        {
            return;
        }

        var admin = await env.Users.FindByEmail(Email.Parse(AdminEmail)!);

        if (admin is null)
        {
            return;
        }

        var ingredients = (await env.Ingredients.All()).ToDictionary(i => i.Name.Value, i => i.Id);

        if (!ingredients.TryGetValue("Mehl", out var mehlId) ||
            !ingredients.TryGetValue("Zucker", out var zuckerId) ||
            !ingredients.TryGetValue("Butter", out var butterId) ||
            !ingredients.TryGetValue("Eier", out var eierId) ||
            !ingredients.TryGetValue("Milch", out var milchId))
        {
            return;
        }

        var kuchenId = await env.Recipes.NextId();
        await env.Recipes.Save(Recipe.Create(
            kuchenId,
            new RecipeName("Einfacher Rührkuchen"),
            new RecipeDescription("Ein klassischer Rührkuchen – saftig, locker und immer beliebt."),
            [
                new PreparationStep(1, "Butter und Zucker cremig rühren."),
                new PreparationStep(2, "Eier einzeln unterrühren."),
                new PreparationStep(3, "Mehl und Milch abwechselnd unterheben."),
                new PreparationStep(4, "Teig in eine gefettete Form füllen und bei 175 °C ca. 40 Minuten backen."),
            ],
            admin.Id,
            Difficulty.Medium,
            [new RecipeTag("Backen"), new RecipeTag("Kuchen")],
            8,
            [
                new RecipeIngredient(mehlId, 250m, "g"),
                new RecipeIngredient(zuckerId, 200m, "g"),
                new RecipeIngredient(butterId, 125m, "g"),
                new RecipeIngredient(eierId, 3m, "Stück"),
                new RecipeIngredient(milchId, 100m, "ml"),
            ],
            [],
            [new RecipeHint("Den Kuchen mit einem Holzstäbchen auf Gare prüfen – bleibt kein Teig kleben, ist er fertig.")]));

        var pfannkuchenId = await env.Recipes.NextId();
        await env.Recipes.Save(Recipe.Create(
            pfannkuchenId,
            new RecipeName("Pfannkuchen"),
            new RecipeDescription("Dünne, goldbraune Pfannkuchen – schnell gemacht und vielseitig belegbar."),
            [
                new PreparationStep(1, "Mehl, Milch und Eier zu einem glatten Teig verrühren."),
                new PreparationStep(2, "Teig 15 Minuten ruhen lassen."),
                new PreparationStep(3, "Butter in einer Pfanne erhitzen und Pfannkuchen portionsweise ausbacken."),
            ],
            admin.Id,
            Difficulty.Easy,
            [new RecipeTag("Frühstück"), new RecipeTag("Pfanne")],
            4,
            [
                new RecipeIngredient(mehlId, 200m, "g"),
                new RecipeIngredient(milchId, 400m, "ml"),
                new RecipeIngredient(eierId, 2m, "Stück"),
                new RecipeIngredient(butterId, 20m, "g"),
            ],
            [],
            [new RecipeHint("Den Teig nicht zu lange rühren – ein paar kleine Klümpchen sind in Ordnung.")]));
    }
}
