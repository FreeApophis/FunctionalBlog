namespace FunctionalBlog.Identity;

public static class CookieHelper
{
    public static string SessionCookie(string token, DateTimeOffset expires) =>
        $"session={token}; HttpOnly; SameSite=Strict; Path=/; Expires={expires:R}; Secure";

    public static string ExpireSessionCookie() =>
        "session=; HttpOnly; SameSite=Strict; Path=/; Max-Age=0";
}
