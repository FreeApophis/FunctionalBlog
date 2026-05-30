namespace FunctionalBlog.Application.Identity;

public interface ISessionStore
{
    ValueTask Save(Session session);

    ValueTask<Session?> Find(string token);

    ValueTask Delete(string token);
}
