using System.Data;
using Microsoft.Data.Sqlite;

namespace FunctionalBlog.DataAccess.Sqlite;

public static class SqliteConnectionFactory
{
    public static IDbConnection Create(string connectionString)
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
        cmd.ExecuteNonQuery();
        return connection;
    }
}
