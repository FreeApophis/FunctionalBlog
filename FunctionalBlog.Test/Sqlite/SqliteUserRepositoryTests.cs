namespace FunctionalBlog.Test.Sqlite;

public sealed class SqliteUserRepositoryTests : UserRepositoryContract, IDisposable
{
    private readonly SqliteTestBase _db = new();

    public void Dispose() => _db.Dispose();

    protected override IUserRepository CreateRepository() => new SqliteUserRepository(_db.Connection);
}
