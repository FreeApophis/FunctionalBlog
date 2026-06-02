namespace FunctionalBlog.Application.Articles;

public interface IArticleRepository
{
    ValueTask<IReadOnlyList<Article>> All();

    ValueTask<Option<Article>> Find(ArticleId id);

    ValueTask<ArticleId> NextId();

    ValueTask Save(Article article);

    ValueTask Delete(ArticleId id);
}
