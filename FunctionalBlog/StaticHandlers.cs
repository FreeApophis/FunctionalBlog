namespace FunctionalBlog;

public static class StaticHandlers
{
    private static readonly Lazy<string> StylesContent = new(LoadStyles);

    public static App Styles => _ => _ =>
        ValueTask.FromResult(Response.Css(StylesContent.Value));

    private static string LoadStyles()
    {
        const string resourceName = "FunctionalBlog.wwwroot.styles.css";
        using var stream = typeof(StaticHandlers).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
