using System.Data;
using Dapper;
using FunctionalBlog.Application.Slugs;
using FunctionalBlog.Domain.Slugs;

namespace FunctionalBlog.DataAccess.Slugs;

public sealed class SqliteSlugRepository : ISlugRepository
{
    private readonly IDbConnection _connection;

    public SqliteSlugRepository(IDbConnection connection) => _connection = connection;

    public async ValueTask<Option<SlugTarget>> FindTarget(string slug)
    {
        var row = await _connection.QuerySingleOrDefaultAsync<TargetRow>(
            "SELECT entity_type AS EntityType, entity_id AS EntityId FROM slugs WHERE slug = @slug",
            new { slug });

        return row is null ? Option<SlugTarget>.None : Option.Some(new SlugTarget(row.EntityType, (int)row.EntityId));
    }

    public async ValueTask<Option<string>> FindSlug(string entityType, int entityId)
    {
        var slug = await _connection.QuerySingleOrDefaultAsync<string>(
            "SELECT slug FROM slugs WHERE entity_type = @entityType AND entity_id = @entityId",
            new { entityType, entityId });

        return Option.FromNullable(slug);
    }

    public async ValueTask<IReadOnlyDictionary<int, string>> SlugsFor(string entityType)
    {
        var rows = await _connection.QueryAsync<SlugRow>(
            "SELECT entity_id AS EntityId, slug AS Slug FROM slugs WHERE entity_type = @entityType",
            new { entityType });

        return rows.ToDictionary(r => (int)r.EntityId, r => r.Slug);
    }

    public async ValueTask Upsert(string entityType, int entityId, string slug)
    {
        await _connection.ExecuteAsync(
            "DELETE FROM slugs WHERE entity_type = @entityType AND entity_id = @entityId",
            new { entityType, entityId });

        await _connection.ExecuteAsync(
            "INSERT INTO slugs (slug, entity_type, entity_id) VALUES (@slug, @entityType, @entityId)",
            new { slug, entityType, entityId });
    }

    private sealed record TargetRow(string EntityType, long EntityId);

    private sealed record SlugRow(long EntityId, string Slug);
}
