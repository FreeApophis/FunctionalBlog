using System.Collections.Concurrent;

namespace FunctionalBlog.DataAccess.Articles;

public sealed class InMemoryArticleRepository : IArticleRepository
{
    private readonly ConcurrentDictionary<int, Article> _articles = new();
    private int _nextId = 1;

    public ValueTask<IReadOnlyList<Article>> All() =>
        ValueTask.FromResult<IReadOnlyList<Article>>(
            _articles.Values.OrderByDescending(x => x.PublishedAt).ToList());

    public ValueTask<Option<Article>> Find(ArticleId id) =>
        ValueTask.FromResult(_articles.GetValueOrNone(id.Value));

    public ValueTask<ArticleId> NextId() =>
        ValueTask.FromResult(new ArticleId(Interlocked.Increment(ref _nextId)));

    public ValueTask Save(Article article)
    {
        _articles[article.Id.Value] = article;
        return ValueTask.CompletedTask;
    }

    public ValueTask Delete(ArticleId id)
    {
        _articles.TryRemove(id.Value, out _);
        return ValueTask.CompletedTask;
    }
}
