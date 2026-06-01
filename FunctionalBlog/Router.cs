using System.Diagnostics.CodeAnalysis;

namespace FunctionalBlog;

public static class Router
{
    public static Middleware Create() => _ => request => env =>
    {
        var app = Match(request) ?? NotFound;
        return app(request)(env);
    };

    private static App? Match(Request request) =>
        (request.Method, request.Path) switch
        {
            ("GET", "/") => BlogHandlers.Index,
            ("GET", "/styles.css") => StaticHandlers.Styles,
            ("GET", "/htmx.min.js") => StaticHandlers.HtmxScript,
            ("GET", "/recipes") => RecipeHandlers.Index,
            ("GET", "/recipes/new") => Auth.RequirePermission<Create>(new RecipeResource(), RecipeHandlers.NewRecipeForm),
            ("POST", "/recipes") => Auth.RequirePermission<Create>(new RecipeResource(), RecipeHandlers.CreateRecipe),
            ("POST", "/recipes/form/ingredients") => Auth.RequirePermission<Create>(new RecipeResource(), RecipeHandlers.IngredientsSection),
            ("POST", "/recipes/form/steps") => Auth.RequirePermission<Create>(new RecipeResource(), RecipeHandlers.StepsSection),
            ("GET", "/articles/new") => Auth.RequirePermission<Create>(new ArticleResource(), BlogHandlers.NewArticleForm),
            ("POST", "/articles") => Auth.RequirePermission<Create>(new ArticleResource(), BlogHandlers.CreateArticle),
            ("GET", "/register") => AuthHandlers.NewRegisterForm,
            ("POST", "/register") => AuthHandlers.Register,
            ("GET", "/login") => AuthHandlers.NewLoginForm,
            ("POST", "/login") => AuthHandlers.Login,
            ("POST", "/logout") => Auth.RequireAuth(AuthHandlers.Logout),
            ("GET", "/password-reset") => AuthHandlers.NewPasswordResetForm,
            ("POST", "/password-reset") => AuthHandlers.RequestPasswordReset,
            ("GET", "/password-reset/confirm") => AuthHandlers.NewPasswordResetConfirmForm,
            ("POST", "/password-reset/confirm") => AuthHandlers.ConfirmPasswordReset,
            ("GET", "/settings") => Auth.RequireAuth(UserSettingsHandlers.Settings),
            ("POST", "/settings/password") => Auth.RequireAuth(UserSettingsHandlers.ChangePassword),
            ("GET", "/admin/ingredients") => Auth.RequirePermission<Manage>(new IngredientResource(), AdminIngredientHandlers.List),
            ("GET", "/admin/ingredients/new") => Auth.RequirePermission<Create>(new IngredientResource(), AdminIngredientHandlers.NewForm),
            ("POST", "/admin/ingredients") => Auth.RequirePermission<Create>(new IngredientResource(), AdminIngredientHandlers.Create),
            ("GET", "/admin/users") => Auth.RequirePermission<Manage>(new UserResource(), AdminHandlers.UserList),
            ("GET", "/admin/roles") => Auth.RequirePermission<Manage>(new RoleResource(), AdminHandlers.RoleList),
            ("GET", "/admin/roles/new") => Auth.RequirePermission<Create>(new RoleResource(), AdminHandlers.NewRoleForm),
            ("POST", "/admin/roles") => Auth.RequirePermission<Create>(new RoleResource(), AdminHandlers.CreateRole),
            ("POST", "/lang") => TranslationHandlers.SetLanguage,
            ("GET", "/admin/translations") => Auth.RequirePermission<Manage>(new UserResource(), TranslationHandlers.List),
            ("GET", "/admin/translations/export.json") => Auth.RequirePermission<Manage>(new UserResource(), TranslationHandlers.Export),
            _ when request.Method == "GET" && TryArticleEditPath(request.Path, out var articleEditId) => Auth.RequirePermission<Edit>(new ArticleResource(), BlogHandlers.EditArticleForm(articleEditId)),
            _ when request.Method == "POST" && TryArticleDeletePath(request.Path, out var articleDeleteId) => Auth.RequirePermission<Delete>(new ArticleResource(), BlogHandlers.DeleteArticle(articleDeleteId)),
            _ when request.Method == "POST" && TryArticlePath(request.Path, out var articleUpdateId) => Auth.RequirePermission<Edit>(new ArticleResource(), BlogHandlers.UpdateArticle(articleUpdateId)),
            _ when request.Method == "POST" && TryRecipeDeletePath(request.Path, out var recipeDeleteId) => Auth.RequirePermission<Delete>(new RecipeResource(), RecipeHandlers.DeleteRecipe(recipeDeleteId)),
            _ when request.Method == "GET" && TryAdminIngredientEditPath(request.Path, out var ingEditId) => Auth.RequirePermission<Edit>(new IngredientResource(), AdminIngredientHandlers.EditForm(ingEditId)),
            _ when request.Method == "POST" && TryAdminIngredientDeletePath(request.Path, out var ingDeleteId) => Auth.RequirePermission<Manage>(new IngredientResource(), AdminIngredientHandlers.Delete(ingDeleteId)),
            _ when request.Method == "POST" && TryAdminIngredientPath(request.Path, out var ingUpdateId) => Auth.RequirePermission<Edit>(new IngredientResource(), AdminIngredientHandlers.Update(ingUpdateId)),
            _ when request.Method == "GET" && TryRecipeEditPath(request.Path, out var editId) => Auth.RequirePermission<Edit>(new RecipeResource(), RecipeHandlers.EditRecipeForm(editId)),
            _ when request.Method == "POST" && TryRecipePath(request.Path, out var updateId) => Auth.RequirePermission<Edit>(new RecipeResource(), RecipeHandlers.UpdateRecipe(updateId)),
            _ when request.Method == "GET" && TryRecipePath(request.Path, out var recipeId) => RecipeHandlers.ShowRecipe(recipeId),
            _ when request.Method == "GET" && TryArticlePath(request.Path, out var id) => BlogHandlers.ShowArticle(id),
            _ when request.Method == "GET" && TryAdminUsersPath(request.Path) => Auth.RequirePermission<Manage>(new UserResource(), AdminHandlers.UserDetail(ExtractAdminUserId(request.Path))),
            _ when request.Method == "POST" && TryAdminUserRolesPath(request.Path) => Auth.RequirePermission<Manage>(new UserResource(), AdminHandlers.UpdateUserRoles(ExtractAdminUserId(request.Path))),
            _ when request.Method == "GET" && TryAdminRolePath(request.Path) => Auth.RequirePermission<Manage>(new RoleResource(), AdminHandlers.RoleDetail(ExtractAdminRoleId(request.Path))),
            _ when request.Method == "POST" && TryAdminRoleRulesPath(request.Path) => Auth.RequirePermission<Manage>(new RuleResource(), AdminHandlers.AddRule(ExtractAdminRoleId(request.Path))),
            _ when request.Method == "POST" && TryAdminRoleDeletePath(request.Path) => Auth.RequirePermission<Manage>(new RoleResource(), AdminHandlers.DeleteRole(ExtractAdminRoleId(request.Path))),
            _ when request.Method == "POST" && TryTranslationSavePath(request.Path, out var tKey, out var tLang) => Auth.RequirePermission<Manage>(new UserResource(), TranslationHandlers.Save(tKey, tLang)),
            _ => null,
        };

    private static bool TryRecipePath(string path, [NotNullWhen(true)] out RecipeId? id)
    {
        id = default;
        const string prefix = "/recipes/";

        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var raw = path[prefix.Length..];

        if (!int.TryParse(raw, out var value))
        {
            return false;
        }

        id = new RecipeId(value);
        return true;
    }

    private static bool TryRecipeEditPath(string path, [NotNullWhen(true)] out RecipeId? id)
    {
        id = default;
        const string prefix = "/recipes/";
        const string suffix = "/edit";

        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var raw = path[prefix.Length..^suffix.Length];

        if (!int.TryParse(raw, out var value))
        {
            return false;
        }

        id = new RecipeId(value);
        return true;
    }

    private static bool TryArticlePath(string path, [NotNullWhen(true)] out ArticleId? id)
    {
        id = default;
        const string prefix = "/articles/";

        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var raw = path[prefix.Length..];

        if (!int.TryParse(raw, out var value))
        {
            return false;
        }

        id = new ArticleId(value);
        return true;
    }

    private static bool TryArticleEditPath(string path, [NotNullWhen(true)] out ArticleId? id)
    {
        id = default;
        const string prefix = "/articles/";
        const string suffix = "/edit";

        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var raw = path[prefix.Length..^suffix.Length];

        if (!int.TryParse(raw, out var value))
        {
            return false;
        }

        id = new ArticleId(value);
        return true;
    }

    private static bool TryAdminUsersPath(string path) =>
        path.StartsWith("/admin/users/", StringComparison.OrdinalIgnoreCase) &&
        int.TryParse(path["/admin/users/".Length..], out _);

    private static bool TryAdminUserRolesPath(string path) =>
        path.StartsWith("/admin/users/", StringComparison.OrdinalIgnoreCase) &&
        path.EndsWith("/roles", StringComparison.OrdinalIgnoreCase);

    private static bool TryAdminRolePath(string path) =>
        path.StartsWith("/admin/roles/", StringComparison.OrdinalIgnoreCase) &&
        int.TryParse(path["/admin/roles/".Length..], out _);

    private static bool TryAdminRoleRulesPath(string path) =>
        path.StartsWith("/admin/roles/", StringComparison.OrdinalIgnoreCase) &&
        path.EndsWith("/rules", StringComparison.OrdinalIgnoreCase);

    private static bool TryAdminRoleDeletePath(string path) =>
        path.StartsWith("/admin/roles/", StringComparison.OrdinalIgnoreCase) &&
        path.EndsWith("/delete", StringComparison.OrdinalIgnoreCase);

    private static int ExtractAdminUserId(string path)
    {
        var segment = path["/admin/users/".Length..].Split('/')[0];
        return int.TryParse(segment, out var id) ? id : 0;
    }

    private static int ExtractAdminRoleId(string path)
    {
        var segment = path["/admin/roles/".Length..].Split('/')[0];
        return int.TryParse(segment, out var id) ? id : 0;
    }

    private static bool TryTranslationSavePath(string path, [NotNullWhen(true)] out string? key, [NotNullWhen(true)] out string? language)
    {
        key = null;
        language = null;
        const string prefix = "/admin/translations/";

        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var rest = path[prefix.Length..];
        var lastSlash = rest.LastIndexOf('/');

        if (lastSlash < 0)
        {
            return false;
        }

        key = rest[..lastSlash];
        language = rest[(lastSlash + 1)..];
        return key.Length > 0 && language.Length > 0;
    }

    private static bool TryArticleDeletePath(string path, [NotNullWhen(true)] out ArticleId? id)
    {
        id = default;
        const string prefix = "/articles/";
        const string suffix = "/delete";

        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var raw = path[prefix.Length..^suffix.Length];

        if (!int.TryParse(raw, out var value))
        {
            return false;
        }

        id = new ArticleId(value);
        return true;
    }

    private static bool TryRecipeDeletePath(string path, [NotNullWhen(true)] out RecipeId? id)
    {
        id = default;
        const string prefix = "/recipes/";
        const string suffix = "/delete";

        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var raw = path[prefix.Length..^suffix.Length];

        if (!int.TryParse(raw, out var value))
        {
            return false;
        }

        id = new RecipeId(value);
        return true;
    }

    private static bool TryAdminIngredientDeletePath(string path, [NotNullWhen(true)] out IngredientId? id)
    {
        id = default;
        const string prefix = "/admin/ingredients/";
        const string suffix = "/delete";

        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var raw = path[prefix.Length..^suffix.Length];

        if (!int.TryParse(raw, out var value))
        {
            return false;
        }

        id = new IngredientId(value);
        return true;
    }

    private static bool TryAdminIngredientPath(string path, [NotNullWhen(true)] out IngredientId? id)
    {
        id = default;
        const string prefix = "/admin/ingredients/";

        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var raw = path[prefix.Length..];

        if (!int.TryParse(raw, out var value))
        {
            return false;
        }

        id = new IngredientId(value);
        return true;
    }

    private static bool TryAdminIngredientEditPath(string path, [NotNullWhen(true)] out IngredientId? id)
    {
        id = default;
        const string prefix = "/admin/ingredients/";
        const string suffix = "/edit";

        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var raw = path[prefix.Length..^suffix.Length];

        if (!int.TryParse(raw, out var value))
        {
            return false;
        }

        id = new IngredientId(value);
        return true;
    }

    private static readonly App NotFound = _ => _ => ValueTask.FromResult(Response.NotFound());
}
