namespace FunctionalBlog.Translations;

public static class LanguageMiddleware
{
    public static Middleware Create() => next => request => env =>
    {
        var lang = request.Cookies.GetValueOrDefault("lang", Languages.Default);
        return next(request)(env with { Language = lang });
    };
}
