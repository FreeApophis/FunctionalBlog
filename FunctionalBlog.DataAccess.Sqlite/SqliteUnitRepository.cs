using System.Data;
using Dapper;

namespace FunctionalBlog.DataAccess.Sqlite;

public sealed class SqliteUnitRepository : IUnitRepository
{
    private readonly IDbConnection _connection;
    private int _nextId = -1;

    public SqliteUnitRepository(IDbConnection connection) => _connection = connection;

    public async ValueTask<IReadOnlyList<Unit>> All()
    {
        var rows = await _connection.QueryAsync<UnitRow>(
            "SELECT id AS Id, category AS Category, unit_factor AS Factor, name_key AS NameKey, abbreviation_key AS AbbreviationKey FROM units ORDER BY id");
        return rows.Select(ToUnit).ToList();
    }

    public async ValueTask<Option<Unit>> Find(UnitId id)
    {
        var row = await _connection.QuerySingleOrDefaultAsync<UnitRow>(
            "SELECT id AS Id, category AS Category, unit_factor AS Factor, name_key AS NameKey, abbreviation_key AS AbbreviationKey FROM units WHERE id = @id",
            new { id = id.Value });

        return Option.FromNullable(row).Select(ToUnit);
    }

    public async ValueTask<UnitId> NextId()
    {
        if (_nextId < 0)
        {
            _nextId = await _connection.ExecuteScalarAsync<int>("SELECT COALESCE(MAX(id), 0) FROM units");
        }

        return new UnitId(++_nextId);
    }

    public async ValueTask Save(Unit unit)
    {
        await _connection.ExecuteAsync(
            """
            INSERT OR REPLACE INTO units (id, category, unit_factor, name_key, abbreviation_key)
            VALUES (@Id, @Category, @Factor, @NameKey, @AbbreviationKey)
            """,
            new
            {
                Id = unit.Id.Value,
                Category = (int)unit.Category,
                unit.Factor,
                unit.NameKey,
                unit.AbbreviationKey,
            });
    }

    public async ValueTask Delete(UnitId id)
    {
        await _connection.ExecuteAsync("DELETE FROM units WHERE id = @id", new { id = id.Value });
    }

    private static Unit ToUnit(UnitRow row) =>
        new(new UnitId((int)row.Id), row.NameKey, row.AbbreviationKey, (UnitCategory)row.Category, row.Factor);

    private sealed record UnitRow(long Id, long Category, decimal Factor, string NameKey, string AbbreviationKey);
}
