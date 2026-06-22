namespace FunctionalBlog;

public sealed record Env(
    IArticleRepository Articles,
    IUserRepository Users,
    IRoleRepository Roles,
    ISessionStore Sessions,
    IPasswordResetTokenStore PasswordResets,
    IPasswordHasher PasswordHasher,
    IClock Clock,
    ILog Log,
    IPrincipal CurrentUser,
    IRecipeRepository Recipes,
    IIngredientRepository Ingredients,
    IImageRepository Images,
    IPageRepository Pages,
    ITranslationRepository? Translations = null,
    TranslationCache? TranslationCache = null,
    ISearchIndex? Search = null,
    string Language = Languages.Default,
    string Theme = "light",
    string CsrfToken = "")
{
    public Translate T =>
        key => TranslationCache?.Get(key, Language) ?? key;

    public ViewContext Ctx => new(CurrentUser, T, CsrfToken, Theme, Language);
}
