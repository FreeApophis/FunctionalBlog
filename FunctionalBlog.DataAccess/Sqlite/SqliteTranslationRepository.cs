using System.Data;
using Dapper;

namespace FunctionalBlog.DataAccess.Sqlite;

public sealed class SqliteTranslationRepository : ITranslationRepository
{
    private readonly IDbConnection _connection;

    public SqliteTranslationRepository(IDbConnection connection) => _connection = connection;

    public async ValueTask<IReadOnlyList<Translation>> All()
    {
        var rows = await _connection.QueryAsync<TranslationRow>(
            "SELECT key, language, variant, text FROM translations");
        return rows.Select(ToTranslation).ToList();
    }

    public async ValueTask Save(string key, string language, string? variant, string text)
    {
        await _connection.ExecuteAsync(
            """
            INSERT INTO translations (key, language, variant, text)
            VALUES (@Key, @Language, @Variant, @Text)
            ON CONFLICT(key, language, variant) DO UPDATE SET text = excluded.text
            """,
            new { Key = key, Language = language, Variant = variant ?? string.Empty, Text = text });
    }

    private static Translation ToTranslation(TranslationRow row) =>
        new(row.Key, row.Language, row.Variant == string.Empty ? null : row.Variant, row.Text);

    private sealed record TranslationRow(string Key, string Language, string Variant, string Text);
}
