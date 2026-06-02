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
        if ((await env.Roles.FindByName(DefaultRoleName)) == Option<Role>.None)
        {
            var id = await env.Roles.NextId();
            var role = Role.Create(id, DefaultRoleName)
                .AddRule(new PermissionRule("View", "article"));
            await env.Roles.Save(role);
        }

        if ((await env.Roles.FindByName(AdminRoleName)) == Option<Role>.None)
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
                .AddRule(new PermissionRule("View", "article"))
                .AddRule(new PermissionRule("Create", "recipe"))
                .AddRule(new PermissionRule("Edit", "recipe"))
                .AddRule(new PermissionRule("Delete", "recipe"))
                .AddRule(new PermissionRule("Manage", "recipe"))
                .AddRule(new PermissionRule("Create", "ingredient"))
                .AddRule(new PermissionRule("Edit", "ingredient"))
                .AddRule(new PermissionRule("Delete", "ingredient"))
                .AddRule(new PermissionRule("Manage", "ingredient"));
            await env.Roles.Save(role);
        }
    }

    private static async ValueTask SeedAdminUser(Env env)
    {
        if (Email.ParseOrNone(AdminEmail) is [var email] && (await env.Users.FindByEmail(email)) is [])
        {
            var id = await env.Users.NextId();
            var hash = env.PasswordHasher.Hash(AdminPassword);
            var user = User.Create(id, email, new DisplayName("Admin"), hash, [AdminRoleName], env.Clock.Now);
            await env.Users.Save(user);
        }
    }

    private static async ValueTask SeedSampleArticles(Env env)
    {
        if ((await env.Articles.All()).Count > 0)
        {
            return;
        }

        var adminEmail = Email.ParseOrNone(AdminEmail).GetOrElse(new Email("invalid"));

        if ((await env.Users.FindByEmail(adminEmail)) is [var admin])
        {
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

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Zwiebel"),
            string.Empty,
            "Gemüsezwiebel, mittelgroß (ca. 150 g)",
            density: 0.75m,
            pieceCount: 150m,
            calorificValue: 40m,
            protein: 1.1m,
            fat: 0.1m,
            carbohydrates: 9.3m,
            sugar: 4.2m,
            fiber: 1.7m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Öl"),
            string.Empty,
            "Neutrales Pflanzenöl",
            density: 0.92m,
            pieceCount: 0m,
            calorificValue: 900m,
            protein: 0m,
            fat: 100m,
            carbohydrates: 0m,
            sugar: 0m,
            fiber: 0m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Paprikapulver"),
            string.Empty,
            "Edelsüßes Paprikapulver",
            density: 0.5m,
            pieceCount: 0m,
            calorificValue: 282m,
            protein: 14m,
            fat: 13m,
            carbohydrates: 54m,
            sugar: 10m,
            fiber: 34m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Kartoffeln"),
            string.Empty,
            "Festkochende Kartoffeln",
            density: 1.1m,
            pieceCount: 0m,
            calorificValue: 77m,
            protein: 2m,
            fat: 0.1m,
            carbohydrates: 17m,
            sugar: 0.8m,
            fiber: 2.2m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Knoblauch"),
            string.Empty,
            "Knoblauchzehe (ca. 5 g)",
            density: 0.95m,
            pieceCount: 5m,
            calorificValue: 149m,
            protein: 6.4m,
            fat: 0.5m,
            carbohydrates: 33m,
            sugar: 1m,
            fiber: 2.1m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Kümmel"),
            string.Empty,
            "Ganze Kümmelsamen",
            density: 0.5m,
            pieceCount: 0m,
            calorificValue: 333m,
            protein: 17m,
            fat: 14.6m,
            carbohydrates: 50m,
            sugar: 1.5m,
            fiber: 10m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Karotten"),
            string.Empty,
            "Möhren, mittelgroß (ca. 100 g)",
            density: 1.0m,
            pieceCount: 100m,
            calorificValue: 41m,
            protein: 0.9m,
            fat: 0.2m,
            carbohydrates: 10m,
            sugar: 4.7m,
            fiber: 2.8m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Gewürzgurken"),
            string.Empty,
            "Eingelegte Gewürzgurken (ca. 50 g pro Stück)",
            density: 1.0m,
            pieceCount: 50m,
            calorificValue: 11m,
            protein: 0.6m,
            fat: 0.1m,
            carbohydrates: 2.4m,
            sugar: 1.1m,
            fiber: 0.5m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Salz"),
            string.Empty,
            "Speisesalz",
            density: 2.16m,
            pieceCount: 0m,
            calorificValue: 0m,
            protein: 0m,
            fat: 0m,
            carbohydrates: 0m,
            sugar: 0m,
            fiber: 0m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Gurkenwasser"),
            string.Empty,
            "Sud aus dem Gewürzgurkenglas",
            density: 1.01m,
            pieceCount: 0m,
            calorificValue: 5m,
            protein: 0m,
            fat: 0m,
            carbohydrates: 1m,
            sugar: 0.5m,
            fiber: 0m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Älplermagronen"),
            string.Empty,
            "Kurze Alpennudeln (Makkaroni)",
            density: 0.65m,
            pieceCount: 0m,
            calorificValue: 365m,
            protein: 12m,
            fat: 1.5m,
            carbohydrates: 73m,
            sugar: 2m,
            fiber: 3m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Äpfel"),
            string.Empty,
            "Säuerliche Äpfel, z. B. Boskoop (ca. 150 g pro Stück)",
            density: 0.85m,
            pieceCount: 150m,
            calorificValue: 52m,
            protein: 0.3m,
            fat: 0.2m,
            carbohydrates: 14m,
            sugar: 10m,
            fiber: 2.4m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Speckwürfel"),
            string.Empty,
            "Geräucherte Speckwürfel",
            density: 0.95m,
            pieceCount: 0m,
            calorificValue: 400m,
            protein: 16m,
            fat: 37m,
            carbohydrates: 0.5m,
            sugar: 0m,
            fiber: 0m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Frühlingszwiebeln"),
            string.Empty,
            "Frische Frühlingszwiebeln (Bund)",
            density: 0.95m,
            pieceCount: 0m,
            calorificValue: 32m,
            protein: 1.8m,
            fat: 0.2m,
            carbohydrates: 7m,
            sugar: 2.3m,
            fiber: 2.6m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Rahm"),
            string.Empty,
            "Vollrahm 35 % Fett",
            density: 0.99m,
            pieceCount: 0m,
            calorificValue: 340m,
            protein: 2.5m,
            fat: 35m,
            carbohydrates: 3m,
            sugar: 3m,
            fiber: 0m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Gemüsebouillon"),
            string.Empty,
            "Klare Gemüsebrühe",
            density: 1.01m,
            pieceCount: 0m,
            calorificValue: 10m,
            protein: 0.3m,
            fat: 0.2m,
            carbohydrates: 1.5m,
            sugar: 0.5m,
            fiber: 0m));

        await env.Ingredients.Save(Ingredient.Create(
            await env.Ingredients.NextId(),
            new IngredientName("Bergkäse"),
            string.Empty,
            "Würziger Schweizer Bergkäse, gerieben",
            density: 1.1m,
            pieceCount: 0m,
            calorificValue: 380m,
            protein: 27m,
            fat: 30m,
            carbohydrates: 0.5m,
            sugar: 0m,
            fiber: 0m));
    }

    private static async ValueTask SeedSampleRecipes(Env env)
    {
        if ((await env.Recipes.All()).Count > 0)
        {
            return;
        }

        var adminEmail = Email.ParseOrNone(AdminEmail).GetOrElse(new Email("invalid"));
        if ((await env.Users.FindByEmail(adminEmail)) is [var admin])
        {
            var ingredients = (await env.Ingredients.All()).ToDictionary(i => i.Name.Value, i => i.Id);

            if (!ingredients.TryGetValue("Mehl", out var mehlId) ||
                !ingredients.TryGetValue("Zucker", out var zuckerId) ||
                !ingredients.TryGetValue("Butter", out var butterId) ||
                !ingredients.TryGetValue("Eier", out var eierId) ||
                !ingredients.TryGetValue("Milch", out var milchId) ||
                !ingredients.TryGetValue("Zwiebel", out var zwiebelId) ||
                !ingredients.TryGetValue("Öl", out var ölId) ||
                !ingredients.TryGetValue("Paprikapulver", out var paprikaId) ||
                !ingredients.TryGetValue("Kartoffeln", out var kartoffelnId) ||
                !ingredients.TryGetValue("Knoblauch", out var knoblauchId) ||
                !ingredients.TryGetValue("Kümmel", out var kümmelId) ||
                !ingredients.TryGetValue("Karotten", out var karottenId) ||
                !ingredients.TryGetValue("Gewürzgurken", out var gewürzgurkenId) ||
                !ingredients.TryGetValue("Salz", out var salzId) ||
                !ingredients.TryGetValue("Gurkenwasser", out var gurkenwasserId) ||
                !ingredients.TryGetValue("Älplermagronen", out var magronenId) ||
                !ingredients.TryGetValue("Äpfel", out var äpfelId) ||
                !ingredients.TryGetValue("Speckwürfel", out var speckId) ||
                !ingredients.TryGetValue("Frühlingszwiebeln", out var frühlingszwiebelnId) ||
                !ingredients.TryGetValue("Rahm", out var rahmId) ||
                !ingredients.TryGetValue("Gemüsebouillon", out var bouillonId) ||
                !ingredients.TryGetValue("Bergkäse", out var käseId))
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
                    new RecipeIngredient(mehlId, 250m, WeightUnit.Gram),
                new RecipeIngredient(zuckerId, 200m, WeightUnit.Gram),
                new RecipeIngredient(butterId, 125m, WeightUnit.Gram),
                new RecipeIngredient(eierId, 3m, PieceUnit.Piece),
                new RecipeIngredient(milchId, 100m, VolumeUnit.Milliliter),
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
                    new RecipeIngredient(mehlId, 200m, WeightUnit.Gram),
                new RecipeIngredient(milchId, 400m, VolumeUnit.Milliliter),
                new RecipeIngredient(eierId, 2m, PieceUnit.Piece),
                new RecipeIngredient(butterId, 20m, WeightUnit.Gram),
                ],
                [],
                [new RecipeHint("Den Teig nicht zu lange rühren – ein paar kleine Klümpchen sind in Ordnung.")]));

            await env.Recipes.Save(Recipe.Create(
                await env.Recipes.NextId(),
                new RecipeName("Kartoffelgulasch"),
                new RecipeDescription("Ein herzhafter Eintopf mit Kartoffeln, Paprika und Gewürzgurken – ein Klassiker der deutschen Hausmannskost."),
                [
                    new PreparationStep(1, "Zwiebel fein würfeln und im Öl in einem großen Topf bei mittlerer Hitze glasig andünsten."),
                new PreparationStep(2, "Kartoffeln schälen und in Würfel schneiden. Mit Paprikapulver, Kümmel und gepresstem Knoblauch in den Topf geben und kurz mitdünsten."),
                new PreparationStep(3, "Mit Wasser bedecken und zum Kochen bringen."),
                new PreparationStep(4, "Karotten in Scheiben und Gewürzgurken in Stücke schneiden. Zusammen mit dem Gurkenwasser und Salz in den Topf geben."),
                new PreparationStep(5, "Bei niedriger bis mittlerer Hitze 60–90 Minuten köcheln lassen, bis die Kartoffeln weich sind."),
                ],
                admin.Id,
                Difficulty.Easy,
                [new RecipeTag("Eintopf"), new RecipeTag("Vegetarisch")],
                4,
                [
                    new RecipeIngredient(zwiebelId, 1m, PieceUnit.Piece),
                new RecipeIngredient(ölId, 1m, VolumeUnit.Tablespoon),
                new RecipeIngredient(paprikaId, 2m, VolumeUnit.Tablespoon),
                new RecipeIngredient(kartoffelnId, 1000m, WeightUnit.Gram),
                new RecipeIngredient(knoblauchId, 3m, PieceUnit.Piece),
                new RecipeIngredient(kümmelId, 1m, VolumeUnit.Tablespoon),
                new RecipeIngredient(karottenId, 2m, PieceUnit.Piece),
                new RecipeIngredient(gewürzgurkenId, 3m, PieceUnit.Piece),
                new RecipeIngredient(salzId, 1m, VolumeUnit.Teaspoon),
                new RecipeIngredient(gurkenwasserId, 1m, VolumeUnit.Tablespoon),
                ],
                [],
                [new RecipeHint("Am zweiten Tag schmeckt das Gericht noch besser – einfach aufwärmen und genießen.")]));

            await env.Recipes.Save(Recipe.Create(
                await env.Recipes.NextId(),
                new RecipeName("Älpler One-Pot"),
                new RecipeDescription("Ein echtes Schweizer Nationalgericht in einer kreativen und schnell gemachten Variante – alles in einem Topf."),
                [
                    new PreparationStep(1, "Magronen, Kartoffeln, Äpfel, Speck, Frühlingszwiebeln und Bouillon in einen großen Topf geben, mischen und zum Kochen bringen. Bei mittlerer Hitze ohne Deckel ca. 20 Minuten köcheln lassen, dabei gelegentlich umrühren."),
                new PreparationStep(2, "Rahm und Bergkäse unterrühren, abschmecken und nach Belieben mit Röstzwiebeln garnieren."),
                ],
                admin.Id,
                Difficulty.Easy,
                [new RecipeTag("Schweizer Küche"), new RecipeTag("One-Pot")],
                4,
                [
                    new RecipeIngredient(magronenId, 400m, WeightUnit.Gram),
                new RecipeIngredient(kartoffelnId, 240m, WeightUnit.Gram),
                new RecipeIngredient(äpfelId, 2m, PieceUnit.Piece),
                new RecipeIngredient(speckId, 160m, WeightUnit.Gram),
                new RecipeIngredient(frühlingszwiebelnId, 1m, PieceUnit.Piece),
                new RecipeIngredient(rahmId, 3m, VolumeUnit.Deciliter),
                new RecipeIngredient(bouillonId, 8m, VolumeUnit.Deciliter),
                new RecipeIngredient(käseId, 79m, WeightUnit.Gram),
                ],
                [],
                [new RecipeHint("Das Gericht nicht zu lange kochen – die Magronen sollen noch bissfest sein.")]));
        }
    }
}
