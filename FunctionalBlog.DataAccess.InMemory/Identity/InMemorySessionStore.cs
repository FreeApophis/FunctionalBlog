using System.Collections.Concurrent;

namespace FunctionalBlog.DataAccess.Identity;

public sealed class InMemorySessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, Session> _sessions = new();

    public ValueTask Save(Session session)
    {
        _sessions[session.Token] = session;
        return ValueTask.CompletedTask;
    }

    public ValueTask<Option<Session>> Find(string token) =>
        ValueTask.FromResult(_sessions.TryGetValue(token, out var session) ? Option.Some(session) : Option<Session>.None);

    public ValueTask Delete(string token)
    {
        _sessions.TryRemove(token, out _);
        return ValueTask.CompletedTask;
    }
}
