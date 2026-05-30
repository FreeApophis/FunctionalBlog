namespace FunctionalBlog.Application.Identity;

public interface IUserRepository
{
    ValueTask<IReadOnlyList<User>> All();

    ValueTask<User?> FindById(UserId id);

    ValueTask<User?> FindByEmail(Email email);

    ValueTask<UserId> NextId();

    ValueTask Save(User user);
}
