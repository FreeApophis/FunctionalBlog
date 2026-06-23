using System.Data;
using Dapper;

namespace FunctionalBlog.DataAccess.Sqlite;

public sealed class SqliteTagRepository : ITagRepository
{
    private readonly IDbConnection _connection;

    public SqliteTagRepository(IDbConnection connection) => _connection = connection;

    public async ValueTask<Option<Tag>> FindBySlug(string slug)
    {
        var row = await _connection.QuerySingleOrDefaultAsync<TagRow>(
            "SELECT slug AS Slug, name AS Name FROM tags WHERE slug = @slug",
            new { slug });

        return Option.FromNullable(row).Select(r => new Tag(r.Slug, r.Name));
    }

    private sealed record TagRow(string Slug, string Name);
}
