using System.Collections.Concurrent;

namespace FunctionalBlog;

public static class StaticHandlers
{
    private static readonly Lazy<string> StylesContent = new(LoadStyles);
    private static readonly Lazy<string> HtmxContent = new(LoadHtmx);
    private static readonly ConcurrentDictionary<string, byte[]> FontCache = new();

    // Self-hosted design fonts (latin subset, variable weight). Whitelist of servable files.
    private static readonly IReadOnlyDictionary<string, string> Fonts = new Dictionary<string, string>
    {
        ["hanken-grotesk.woff2"] = "FunctionalBlog.wwwroot.fonts.hanken-grotesk.woff2",
        ["jetbrains-mono.woff2"] = "FunctionalBlog.wwwroot.fonts.jetbrains-mono.woff2",
        ["newsreader.woff2"] = "FunctionalBlog.wwwroot.fonts.newsreader.woff2",
        ["newsreader-italic.woff2"] = "FunctionalBlog.wwwroot.fonts.newsreader-italic.woff2",
    };

    private static readonly IReadOnlyDictionary<string, string> FontHeaders =
        new Dictionary<string, string> { ["Cache-Control"] = "public, max-age=31536000, immutable" };

    public static App Styles => _ => _ =>
        ValueTask.FromResult(Response.Css(StylesContent.Value));

    public static App HtmxScript => _ => _ =>
        ValueTask.FromResult(Response.Js(HtmxContent.Value));

    public static App Font(string file) => _ => _ =>
        Fonts.GetValueOrNone(file) is [var resourceName]
            ? ValueTask.FromResult(Response.Bytes(
                "font/woff2",
                FontCache.GetOrAdd(resourceName, LoadResourceBytes),
                FontHeaders))
            : ValueTask.FromResult(Response.NotFound());

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

    private static byte[] LoadResourceBytes(string resourceName)
    {
        using var stream = typeof(StaticHandlers).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
