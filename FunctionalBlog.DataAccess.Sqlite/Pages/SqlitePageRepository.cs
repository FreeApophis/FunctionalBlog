using System.Data;
using Dapper;

namespace FunctionalBlog.DataAccess.Pages;

public sealed class SqlitePageRepository : IPageRepository
{
    private readonly IDbConnection _connection;
    private int _nextId = -1;

    public SqlitePageRepository(IDbConnection connection) => _connection = connection;

    public async ValueTask<IReadOnlyList<Page>> All()
    {
        var rows = await _connection.QueryAsync<PageRow>(
            "SELECT id AS Id, title AS Title, content AS Content FROM pages ORDER BY id");
        return rows.Select(ToPage).ToList();
    }

    public async ValueTask<Option<Page>> Find(PageId id)
    {
        var row = await _connection.QuerySingleOrDefaultAsync<PageRow>(
            "SELECT id AS Id, title AS Title, content AS Content FROM pages WHERE id = @id",
            new { id = id.Value });

        return Option.FromNullable(row).Select(ToPage);
    }

    public async ValueTask<PageId> NextId()
    {
        if (_nextId < 0)
        {
            _nextId = await _connection.ExecuteScalarAsync<int>("SELECT COALESCE(MAX(id), 0) FROM pages");
        }

        return new PageId(++_nextId);
    }

    public async ValueTask Save(Page page)
    {
        await _connection.ExecuteAsync(
            """
            INSERT OR REPLACE INTO pages (id, title, content)
            VALUES (@Id, @Title, @Content)
            """,
            new
            {
                Id = page.Id.Value,
                Title = page.Title.Value,
                Content = page.Content.Value,
            });
    }

    public async ValueTask Delete(PageId id)
    {
        await _connection.ExecuteAsync("DELETE FROM pages WHERE id = @id", new { id = id.Value });
    }

    private static Page ToPage(PageRow row) =>
        Page.Create(new PageId((int)row.Id), new PageTitle(row.Title), new PageContent(row.Content));

    private sealed record PageRow(long Id, string Title, string Content);
}
