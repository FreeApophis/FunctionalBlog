namespace FunctionalBlog;

public static class StaticHandlers
{
    private static readonly Lazy<string> StylesContent = new(LoadStyles);
    private static readonly Lazy<string> HtmxContent = new(LoadHtmx);

    public static App Styles => _ => _ =>
        ValueTask.FromResult(Response.Css(StylesContent.Value));

    public static App HtmxScript => _ => _ =>
        ValueTask.FromResult(Response.Js(HtmxContent.Value));

    private static string LoadStyles()
    {
        const string resourceName = "FunctionalBlog.wwwroot.styles.css";
        return LoadResource(resourceName);
    }

    private static string LoadHtmx()
    {
        const string resourceName = "FunctionalBlog.wwwroot.htmx.min.js";
        return LoadResource(resourceName);
    }

    private static string LoadResource(string resourceName)
    {
        using var stream = typeof(StaticHandlers).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
