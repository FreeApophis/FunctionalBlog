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
    IPrincipal CurrentUser);
