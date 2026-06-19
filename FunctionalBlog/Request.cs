namespace FunctionalBlog;

public sealed record Request(
    HttpMethod Method,
    string Path,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> Query,
    IReadOnlyDictionary<string, string> Form,
    IReadOnlyDictionary<string, string> Cookies)
{
    public IReadOnlyList<UploadedFile> Files { get; init; } = [];
}
