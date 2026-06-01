using System.Data;
using Microsoft.Data.Sqlite;

namespace FunctionalBlog.Test.Sqlite;

public sealed class SqliteTestBase : IDisposable
{
    private readonly string _dbPath;

    public IDbConnection Connection { get; }

    public SqliteTestBase()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.db");
        DatabaseMigrator.Migrate($"Data Source={_dbPath}");
        SqliteConnection.ClearAllPools();

        var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys=ON;";
        cmd.ExecuteNonQuery();
        Connection = connection;
    }

    public void Dispose()
    {
        Connection.Close();
        Connection.Dispose();
        SqliteConnection.ClearAllPools();

        foreach (var path in new[] { _dbPath, _dbPath + "-wal", _dbPath + "-shm" })
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
            }
        }
    }
}
