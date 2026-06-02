using System.Data;
using Dapper;

namespace FunctionalBlog.DataAccess.Sqlite;

public sealed class SqlitePasswordResetTokenStore : IPasswordResetTokenStore
{
    private readonly IDbConnection _connection;

    public SqlitePasswordResetTokenStore(IDbConnection connection) => _connection = connection;

    public async ValueTask Save(PasswordResetToken token)
    {
        await _connection.ExecuteAsync(
            "INSERT OR REPLACE INTO password_reset_tokens (token, user_id, expires_at, consumed) VALUES (@Token, @UserId, @ExpiresAt, @Consumed)",
            new { token.Token, UserId = token.UserId.Value, token.ExpiresAt, Consumed = token.Consumed ? 1 : 0 });
    }

    public async ValueTask<Option<PasswordResetToken>> Find(string token)
    {
        var row = await _connection.QuerySingleOrDefaultAsync<TokenRow>(
            "SELECT token AS Token, user_id AS UserId, expires_at AS ExpiresAt, consumed AS Consumed FROM password_reset_tokens WHERE token = @token",
            new { token });
        return Option.FromNullable(row).Select(r => new PasswordResetToken(r.Token, new UserId((int)r.UserId), r.ExpiresAt, r.Consumed != 0L));
    }

    public async ValueTask Consume(string token)
    {
        await _connection.ExecuteAsync(
            "UPDATE password_reset_tokens SET consumed = 1 WHERE token = @token",
            new { token });
    }

    private sealed record TokenRow(string Token, long UserId, DateTimeOffset ExpiresAt, long Consumed);
}
