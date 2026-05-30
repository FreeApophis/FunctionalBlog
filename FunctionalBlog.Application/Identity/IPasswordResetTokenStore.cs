namespace FunctionalBlog.Application.Identity;

public interface IPasswordResetTokenStore
{
    ValueTask Save(PasswordResetToken token);

    ValueTask<PasswordResetToken?> Find(string token);

    ValueTask Consume(string token);
}
