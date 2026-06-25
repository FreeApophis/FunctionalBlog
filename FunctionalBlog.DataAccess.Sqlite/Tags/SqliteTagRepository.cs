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
            "SELECT s.slug AS Slug, t.name AS Name " +
            "FROM slugs s JOIN tags t ON t.id = s.entity_id " +
            "WHERE s.entity_type = 'tag' AND s.slug = @slug",
            new { slug });

        return Option.FromNullable(row).Select(r => new Tag(r.Slug, r.Name));
    }

    public async ValueTask<IReadOnlyList<TagEntry>> All()
    {
        var rows = await _connection.QueryAsync<TagEntryRow>("SELECT id AS Id, name AS Name FROM tags");
        return rows.Select(r => new TagEntry((int)r.Id, r.Name)).ToList();
    }

    public async ValueTask<Option<int>> FindIdByName(string name)
    {
        var id = await _connection.ExecuteScalarAsync<long?>(
            "SELECT id FROM tags WHERE name = @name COLLATE NOCASE ORDER BY id LIMIT 1",
            new { name });

        return id is { } value ? Option.Some((int)value) : Option<int>.None;
    }

    private sealed record TagRow(string Slug, string Name);

    private sealed record TagEntryRow(long Id, string Name);
}
