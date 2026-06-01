using System.Data;
using Dapper;

namespace FunctionalBlog.DataAccess.Sqlite;

public sealed class SqliteUserRepository : IUserRepository
{
    private readonly IDbConnection _connection;
    private int _nextId = -1;

    public SqliteUserRepository(IDbConnection connection) => _connection = connection;

    public async ValueTask<IReadOnlyList<User>> All()
    {
        var rows = (await _connection.QueryAsync<UserRow>(
            "SELECT id AS Id, email AS Email, display_name AS DisplayName, password_hash AS PasswordHash, created_at AS CreatedAt FROM users")).ToList();
        if (rows.Count == 0)
        {
            return [];
        }

        var ids = rows.Select(r => r.Id).ToList();
        var roleLookup = (await _connection.QueryAsync<RoleNameRow>(
            "SELECT user_id AS UserId, role_name AS RoleName FROM user_roles WHERE user_id IN @ids",
            new { ids })).ToLookup(r => r.UserId, r => r.RoleName);

        return rows.Select(r => ToUser(r, roleLookup[r.Id].ToList())).ToList();
    }

    public async ValueTask<User?> FindById(UserId id)
    {
        var row = await _connection.QuerySingleOrDefaultAsync<UserRow>(
            "SELECT id AS Id, email AS Email, display_name AS DisplayName, password_hash AS PasswordHash, created_at AS CreatedAt FROM users WHERE id = @id",
            new { id = id.Value });
        if (row is null)
        {
            return null;
        }

        var roles = (await _connection.QueryAsync<string>(
            "SELECT role_name FROM user_roles WHERE user_id = @id",
            new { id = id.Value })).ToList();
        return ToUser(row, roles);
    }

    public async ValueTask<User?> FindByEmail(Email email)
    {
        var row = await _connection.QuerySingleOrDefaultAsync<UserRow>(
            "SELECT id AS Id, email AS Email, display_name AS DisplayName, password_hash AS PasswordHash, created_at AS CreatedAt FROM users WHERE email = @email",
            new { email = email.Value });
        if (row is null)
        {
            return null;
        }

        var roles = (await _connection.QueryAsync<string>(
            "SELECT role_name FROM user_roles WHERE user_id = @id",
            new { id = row.Id })).ToList();
        return ToUser(row, roles);
    }

    public async ValueTask<UserId> NextId()
    {
        if (_nextId < 0)
        {
            _nextId = await _connection.ExecuteScalarAsync<int>("SELECT COALESCE(MAX(id), 0) FROM users");
        }

        return new UserId(++_nextId);
    }

    public async ValueTask Save(User user)
    {
        await _connection.ExecuteAsync(
            """
            INSERT INTO users (id, email, display_name, password_hash, created_at)
            VALUES (@Id, @Email, @DisplayName, @PasswordHash, @CreatedAt)
            ON CONFLICT(id) DO UPDATE SET
                email         = excluded.email,
                display_name  = excluded.display_name,
                password_hash = excluded.password_hash,
                created_at    = excluded.created_at
            """,
            new
            {
                Id = user.Id.Value,
                Email = user.Email.Value,
                DisplayName = user.DisplayName.Value,
                user.PasswordHash,
                user.CreatedAt,
            });

        await _connection.ExecuteAsync(
            "DELETE FROM user_roles WHERE user_id = @id",
            new { id = user.Id.Value });

        if (user.RoleNames.Count > 0)
        {
            await _connection.ExecuteAsync(
                "INSERT INTO user_roles (user_id, role_name) VALUES (@UserId, @RoleName)",
                user.RoleNames.Select(r => new { UserId = user.Id.Value, RoleName = r }));
        }
    }

    private static User ToUser(UserRow row, IReadOnlyList<string> roles) =>
        User.Create(
            new UserId((int)row.Id),
            new Email(row.Email),
            new DisplayName(row.DisplayName),
            row.PasswordHash,
            roles,
            row.CreatedAt);

    private sealed record UserRow(long Id, string Email, string DisplayName, string PasswordHash, DateTimeOffset CreatedAt);

    private sealed record RoleNameRow(long UserId, string RoleName);
}
