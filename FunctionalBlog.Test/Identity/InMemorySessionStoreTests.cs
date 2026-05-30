namespace FunctionalBlog.Test.Identity;

public sealed class InMemorySessionStoreTests : SessionStoreContract
{
    protected override ISessionStore CreateStore() => new InMemorySessionStore();
}
