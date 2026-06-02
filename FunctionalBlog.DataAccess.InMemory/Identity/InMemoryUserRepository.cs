using System.Collections.Concurrent;

namespace FunctionalBlog.DataAccess.Identity;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<int, User> _users = new();
    private int _nextId = 1;

    public ValueTask<IReadOnlyList<User>> All() =>
        ValueTask.FromResult<IReadOnlyList<User>>(_users.Values.ToList());

    public ValueTask<Option<User>> FindById(UserId id) =>
        ValueTask.FromResult(_users.TryGetValue(id.Value, out var user) ? Option.Some(user) : Option<User>.None);

    public ValueTask<Option<User>> FindByEmail(Email email)
    {
        var user = _users.Values.FirstOrDefault(u => u.Email.Value == email.Value);
        return ValueTask.FromResult(user is not null ? Option.Some(user) : Option<User>.None);
    }

    public ValueTask<UserId> NextId() =>
        ValueTask.FromResult(new UserId(Interlocked.Increment(ref _nextId)));

    public ValueTask Save(User user)
    {
        _users[user.Id.Value] = user;
        return ValueTask.CompletedTask;
    }
}
