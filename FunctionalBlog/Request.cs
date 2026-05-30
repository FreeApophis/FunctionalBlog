namespace FunctionalBlog;

public sealed record Request(
    string Method,
    string Path,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> Query,
    IReadOnlyDictionary<string, string> Form,
    IReadOnlyDictionary<string, string> Cookies);
