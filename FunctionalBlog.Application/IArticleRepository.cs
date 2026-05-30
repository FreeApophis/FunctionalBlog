public interface IArticleRepository
{
    ValueTask<IReadOnlyList<Article>> All();
    ValueTask<Article?> Find(ArticleId id);
    ValueTask<ArticleId> NextId();
    ValueTask Save(Article article);
}
