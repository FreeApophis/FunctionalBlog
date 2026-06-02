namespace FunctionalBlog.Test.Identity;

public abstract class UserRepositoryContract
{
    [Fact]
    public async Task Save_then_FindById_returns_the_saved_user()
    {
        var repo = CreateRepository();
        var user = AUser(await repo.NextId());

        await repo.Save(user);

        Assert.Equal(Option.Some(user), await repo.FindById(user.Id));
    }

    [Fact]
    public async Task FindById_returns_null_for_an_unknown_id()
    {
        var repo = CreateRepository();

        Assert.Equal(Option<User>.None, await repo.FindById(new UserId(987_654)));
    }

    [Fact]
    public async Task Save_then_FindByEmail_returns_the_saved_user()
    {
        var repo = CreateRepository();
        var user = AUser(await repo.NextId());

        await repo.Save(user);

        Assert.Equal(Option.Some(user), await repo.FindByEmail(user.Email));
    }

    [Fact]
    public async Task FindByEmail_returns_null_for_an_unknown_email()
    {
        var repo = CreateRepository();

        Assert.Equal(Option<User>.None, await repo.FindByEmail(new Email("nobody@example.com")));
    }

    [Fact]
    public async Task NextId_returns_distinct_values_across_calls()
    {
        var repo = CreateRepository();

        var first = await repo.NextId();
        var second = await repo.NextId();

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task All_returns_all_saved_users()
    {
        var repo = CreateRepository();
        var userA = AUser(await repo.NextId(), "a@blog.de");
        var userB = AUser(await repo.NextId(), "b@blog.de");

        await repo.Save(userA);
        await repo.Save(userB);

        var all = await repo.All();
        Assert.Contains(userA, all);
        Assert.Contains(userB, all);
    }

    [Fact]
    public async Task Save_replaces_an_existing_user_with_the_same_id()
    {
        var repo = CreateRepository();
        var id = await repo.NextId();
        var original = AUser(id, "original@blog.de");
        var updated = AUser(id, "updated@blog.de");

        await repo.Save(original);
        await repo.Save(updated);

        Assert.Equal(Option.Some(updated), await repo.FindById(id));
    }

    protected abstract IUserRepository CreateRepository();

    private static User AUser(UserId id, string email = "test@blog.de") =>
        User.Create(
            id,
            new Email(email),
            new DisplayName("Testbenutzer"),
            "hash",
            [],
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
}
