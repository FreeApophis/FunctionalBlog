namespace FunctionalBlog.Articles;

public sealed record DecodedArticleForm(
    bool IsValid,
    IReadOnlyList<string> Errors,
    string Title,
    string Teaser,
    string Text);
