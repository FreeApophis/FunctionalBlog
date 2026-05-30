using System.Collections.Concurrent;

namespace FunctionalBlog.DataAccess.Articles;

public sealed class InMemoryArticleRepository : IArticleRepository
{
    private readonly ConcurrentDictionary<int, Article> _articles = new();
    private int _nextId = 3;

    public InMemoryArticleRepository()
    {
        _articles[1] = Article.Create(
            new ArticleId(1),
            new ArticleTitle("Hallo funktionales Blog"),
            new ArticleText("Dieser Blog wurde mit einem funktionalen Ansatz in .NET 10 entwickelt."),
            DateTimeOffset.UtcNow);

        _articles[2] = Article.Create(
            new ArticleId(2),
            new ArticleTitle("Macarons selbst backen"),
            new ArticleText("Macarons sind kleine französische Mandelbaisers mit einer Cremefüllung."),
            DateTimeOffset.UtcNow);
    }

    public ValueTask<IReadOnlyList<Article>> All() =>
        ValueTask.FromResult<IReadOnlyList<Article>>(
            _articles.Values.OrderByDescending(x => x.CreatedAt).ToList());

    public ValueTask<Article?> Find(ArticleId id) =>
        ValueTask.FromResult(_articles.TryGetValue(id.Value, out var article) ? article : null);

    public ValueTask<ArticleId> NextId() =>
        ValueTask.FromResult(new ArticleId(Interlocked.Increment(ref _nextId)));

    public ValueTask Save(Article article)
    {
        _articles[article.Id.Value] = article;
        return ValueTask.CompletedTask;
    }
}
