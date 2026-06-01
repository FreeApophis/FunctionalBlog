using System.Data;
using Dapper;

namespace FunctionalBlog.DataAccess.Sqlite;

public sealed class SqliteArticleRepository : IArticleRepository
{
    private readonly IDbConnection _connection;
    private int _nextId = -1;

    public SqliteArticleRepository(IDbConnection connection) => _connection = connection;

    public async ValueTask<IReadOnlyList<Article>> All()
    {
        var rows = await _connection.QueryAsync<ArticleRow>(
            "SELECT id AS Id, title AS Title, teaser AS Teaser, text AS Text, author_id AS AuthorId, published_at AS PublishedAt FROM articles ORDER BY published_at DESC");
        return rows.Select(ToArticle).ToList();
    }

    public async ValueTask<Article?> Find(ArticleId id)
    {
        var row = await _connection.QuerySingleOrDefaultAsync<ArticleRow>(
            "SELECT id AS Id, title AS Title, teaser AS Teaser, text AS Text, author_id AS AuthorId, published_at AS PublishedAt FROM articles WHERE id = @id",
            new { id = id.Value });
        return row is null ? null : ToArticle(row);
    }

    public async ValueTask<ArticleId> NextId()
    {
        if (_nextId < 0)
        {
            _nextId = await _connection.ExecuteScalarAsync<int>("SELECT COALESCE(MAX(id), 0) FROM articles");
        }

        return new ArticleId(++_nextId);
    }

    public async ValueTask Save(Article article)
    {
        await _connection.ExecuteAsync(
            """
            INSERT OR REPLACE INTO articles (id, title, teaser, text, author_id, published_at)
            VALUES (@Id, @Title, @Teaser, @Text, @AuthorId, @PublishedAt)
            """,
            new
            {
                Id = article.Id.Value,
                Title = article.Title.Value,
                Teaser = article.Teaser.Value,
                Text = article.Text.Value,
                AuthorId = article.AuthorId.Value,
                article.PublishedAt,
            });
    }

    public async ValueTask Delete(ArticleId id)
    {
        await _connection.ExecuteAsync("DELETE FROM articles WHERE id = @id", new { id = id.Value });
    }

    private static Article ToArticle(ArticleRow row) =>
        Article.Create(
            new ArticleId((int)row.Id),
            new ArticleTitle(row.Title),
            new ArticleTeaser(row.Teaser),
            new ArticleText(row.Text),
            new UserId((int)row.AuthorId),
            row.PublishedAt);

    private sealed record ArticleRow(long Id, string Title, string Teaser, string Text, long AuthorId, DateTimeOffset PublishedAt);
}
