namespace FunctionalBlog.Test.Identity;

public abstract class SessionStoreContract
{
    [Fact]
    public async Task Save_then_Find_returns_the_saved_session()
    {
        var store = CreateStore();
        var session = ASession("tok1");

        await store.Save(session);

        Assert.Equal(Option.Some(session), await store.Find("tok1"));
    }

    [Fact]
    public async Task Find_returns_null_for_an_unknown_token()
    {
        var store = CreateStore();

        Assert.Equal(Option<Session>.None, await store.Find("unknown"));
    }

    [Fact]
    public async Task Delete_removes_the_session()
    {
        var store = CreateStore();
        var session = ASession("tok2");

        await store.Save(session);
        await store.Delete("tok2");

        Assert.Equal(Option<Session>.None, await store.Find("tok2"));
    }

    protected abstract ISessionStore CreateStore();

    private static Session ASession(string token) =>
        new(token, new UserId(1), DateTimeOffset.UtcNow.AddDays(30));
}
