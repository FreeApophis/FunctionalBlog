namespace FunctionalBlog;

public static class Functional
{
    public static App Compose(params Middleware[] middlewares)
    {
        App app = _ => _ => ValueTask.FromResult(Response.NotFound());

        for (var i = middlewares.Length - 1; i >= 0; i--)
        {
            app = middlewares[i](app);
        }

        return app;
    }
}
