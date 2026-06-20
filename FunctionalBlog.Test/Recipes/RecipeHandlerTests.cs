namespace FunctionalBlog.Test.Recipes;

public sealed class RecipeHandlerTests
{
    private static readonly byte[] PngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01];
    private static readonly byte[] JpegBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

    [Fact]
    public async Task CreateRecipe_uploads_images_to_the_library_and_links_their_urls()
    {
        var env = BuildEnv();
        var request = ARecipeRequest("/recipes") with
        {
            Files =
            [
                new UploadedFile("images", "a.png", "image/png", PngBytes),
                new UploadedFile("images", "b.jpg", "image/jpeg", JpegBytes),
            ],
        };

        var response = await RecipeHandlers.CreateRecipe(request)(env);

        Assert.Equal(303, response.Status);
        var recipe = Assert.Single(await env.Recipes.All());
        Assert.Equal(2, recipe.Images.Count);
        Assert.All(recipe.Images, url => Assert.StartsWith("/images/", url));
        Assert.Equal(2, (await env.Images.List()).Count);
    }

    [Fact]
    public async Task CreateRecipe_rejects_an_invalid_image_and_saves_nothing()
    {
        var env = BuildEnv();
        var request = ARecipeRequest("/recipes") with
        {
            Files = [new UploadedFile("images", "bad.exe", "image/png", [0x4D, 0x5A, 0x90])],
        };

        var response = await RecipeHandlers.CreateRecipe(request)(env);

        Assert.Equal(400, response.Status);
        Assert.Empty(await env.Recipes.All());
        Assert.Empty(await env.Images.List());
    }

    [Fact]
    public async Task UpdateRecipe_keeps_unremoved_images_and_appends_newly_uploaded_ones()
    {
        var env = BuildEnv();
        var id = await SeedRecipe(env, ["/images/1"]);
        var form = RecipeForm(("existing_image_0", "/images/1"));
        var request = new Request(HttpMethod.Post, $"/recipes/{id.Value}", Empty, Empty, form, Empty)
        {
            Files = [new UploadedFile("images", "new.png", "image/png", PngBytes)],
        };

        var response = await RecipeHandlers.UpdateRecipe(id)(request)(env);

        Assert.Equal(303, response.Status);
        var recipe = FunctionalAssert.Some(await env.Recipes.Find(id));
        Assert.Equal(2, recipe.Images.Count);
        Assert.Equal("/images/1", recipe.Images[0]);
        Assert.StartsWith("/images/", recipe.Images[1]);
    }

    [Fact]
    public async Task UpdateRecipe_drops_existing_images_marked_for_removal()
    {
        var env = BuildEnv();
        var id = await SeedRecipe(env, ["/images/1"]);
        var form = RecipeForm(("existing_image_0", "/images/1"), ("remove_image_0", "on"));
        var request = new Request(HttpMethod.Post, $"/recipes/{id.Value}", Empty, Empty, form, Empty);

        var response = await RecipeHandlers.UpdateRecipe(id)(request)(env);

        Assert.Equal(303, response.Status);
        var recipe = FunctionalAssert.Some(await env.Recipes.Find(id));
        Assert.Empty(recipe.Images);
    }

    [Fact]
    public async Task ShowRecipe_renders_a_css_slider_with_every_image()
    {
        var env = BuildEnv();
        var id = await SeedRecipe(env, ["/images/3", "/images/4"]);

        var response = await RecipeHandlers.ShowRecipe(id)(AnEmptyRequest())(env);

        Assert.Contains("slider-track", response.Body);
        Assert.Contains("/images/3", response.Body);
        Assert.Contains("/images/4", response.Body);
    }

    private static async Task<RecipeId> SeedRecipe(Env env, IReadOnlyList<string> images)
    {
        var id = await env.Recipes.NextId();
        await env.Recipes.Save(Recipe.Create(
            id,
            new RecipeName("Rührkuchen"),
            new RecipeDescription("Ein klassischer Rührkuchen."),
            [new PreparationStep(1, "Alles verrühren.")],
            new UserId(1),
            Difficulty.Easy,
            [],
            4,
            [],
            images,
            []));
        return id;
    }

    private static Request ARecipeRequest(string path) =>
        new(HttpMethod.Post, path, Empty, Empty, RecipeForm(), Empty);

    private static Dictionary<string, string> RecipeForm(params (string Key, string Value)[] extra)
    {
        var form = new Dictionary<string, string>
        {
            ["name"] = "Rührkuchen",
            ["description"] = "Ein klassischer Rührkuchen.",
            ["portions"] = "4",
            ["difficulty"] = "0",
            ["step_0"] = "Alles verrühren.",
        };

        foreach (var (key, value) in extra)
        {
            form[key] = value;
        }

        return form;
    }

    private static Request AnEmptyRequest() =>
        new(HttpMethod.Get, "/", Empty, Empty, Empty, Empty);

    private static AuthenticatedUser AuthUser()
    {
        var user = User.Create(
            new UserId(1),
            new Email("admin@blog.de"),
            new DisplayName("Admin"),
            "hash",
            [],
            DateTimeOffset.UtcNow);
        return new AuthenticatedUser(user, []);
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
        CurrentUser: AuthUser(),
        Recipes: new InMemoryRecipeRepository(),
        Ingredients: new InMemoryIngredientRepository(),
        Images: new InMemoryImageRepository(),
        Pages: new InMemoryPageRepository());

    private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();
}
