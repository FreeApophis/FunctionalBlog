namespace FunctionalBlog.Domain.Articles;

public sealed record Article(
    ArticleId Id,
    ArticleTitle Title,
    ArticleText Text,
    DateTimeOffset CreatedAt)
{
    public static Article Create(ArticleId id, ArticleTitle title, ArticleText text, DateTimeOffset createdAt) =>
        new(id, title, text, createdAt);
}
