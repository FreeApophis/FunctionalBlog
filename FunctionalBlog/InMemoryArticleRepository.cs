using System.Collections.Concurrent;

public sealed class InMemoryArticleRepository : IArticleRepository
{
    private readonly ConcurrentDictionary<int, Article> _articles = new();
    private int _nextId = 2;

    public InMemoryArticleRepository()
    {
        _articles[1] = Article.Create(
            new ArticleId(1),
            new ArticleTitle("Hallo funktionales Blog"),
            new ArticleText("Dies ist der erste Artikel. Die Anwendung ist absichtlich klein, aber funktional aufgebaut."),
            DateTimeOffset.UtcNow
        );
    }

    public ValueTask<IReadOnlyList<Article>> All()
    {
        var articles = _articles.Values
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return ValueTask.FromResult<IReadOnlyList<Article>>(articles);
    }

    public ValueTask<Article?> Find(ArticleId id)
    {
        _articles.TryGetValue(id.Value, out var article);
        return ValueTask.FromResult(article);
    }

    public ValueTask<ArticleId> NextId()
    {
        var id = Interlocked.Increment(ref _nextId);
        return ValueTask.FromResult(new ArticleId(id));
    }

    public ValueTask Save(Article article)
    {
        _articles[article.Id.Value] = article;
        return ValueTask.CompletedTask;
    }
}
