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
    ITranslationRepository? Translations = null,
    TranslationCache? TranslationCache = null,
    ISearchIndex? Search = null,
    string Language = Languages.Default)
{
    public Translate T =>
        key => TranslationCache?.Get(key, Language) ?? key;
}
