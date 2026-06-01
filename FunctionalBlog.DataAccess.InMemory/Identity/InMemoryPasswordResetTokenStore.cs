using System.Collections.Concurrent;

namespace FunctionalBlog.DataAccess.Identity;

public sealed class InMemoryPasswordResetTokenStore : IPasswordResetTokenStore
{
    private readonly ConcurrentDictionary<string, PasswordResetToken> _tokens = new();

    public ValueTask Save(PasswordResetToken token)
    {
        _tokens[token.Token] = token;
        return ValueTask.CompletedTask;
    }

    public ValueTask<PasswordResetToken?> Find(string token) =>
        ValueTask.FromResult(_tokens.TryGetValue(token, out var t) ? t : null);

    public ValueTask Consume(string token)
    {
        while (_tokens.TryGetValue(token, out var existing))
        {
            if (_tokens.TryUpdate(token, existing with { Consumed = true }, existing))
            {
                break;
            }
        }

        return ValueTask.CompletedTask;
    }
}
