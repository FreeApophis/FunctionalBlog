using System.Data;
using Dapper;
using FunctionalBlog.Application.Configuration;

namespace FunctionalBlog.DataAccess.Sqlite;

public sealed class SqliteConfigurationRepository : IConfigurationRepository
{
    private readonly IDbConnection _connection;

    public SqliteConfigurationRepository(IDbConnection connection) => _connection = connection;

    public async ValueTask<IReadOnlyDictionary<string, string>> All()
    {
        var rows = await _connection.QueryAsync<ConfigRow>("SELECT key AS Key, value AS Value FROM configuration");
        return rows.ToDictionary(r => r.Key, r => r.Value);
    }

    public async ValueTask<Option<string>> Get(string key)
    {
        var value = await _connection.QuerySingleOrDefaultAsync<string?>(
            "SELECT value FROM configuration WHERE key = @key", new { key });
        return Option.FromNullable(value);
    }

    public async ValueTask Set(string key, string value)
    {
        await _connection.ExecuteAsync(
            """
            INSERT INTO configuration (key, value) VALUES (@key, @value)
            ON CONFLICT(key) DO UPDATE SET value = excluded.value
            """,
            new { key, value });
    }

    private sealed record ConfigRow(string Key, string Value);
}
