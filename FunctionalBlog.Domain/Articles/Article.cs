namespace FunctionalBlog.Domain.Articles;

public sealed record Article(
    ArticleId Id,
    ArticleTitle Title,
    ArticleTeaser Teaser,
    ArticleText Text,
    UserId AuthorId,
    DateTimeOffset PublishedAt)
{
    public static Article Create(
        ArticleId id,
        ArticleTitle title,
        ArticleTeaser teaser,
        ArticleText text,
        UserId authorId,
        DateTimeOffset publishedAt) =>
        new(id, title, teaser, text, authorId, publishedAt);
}
