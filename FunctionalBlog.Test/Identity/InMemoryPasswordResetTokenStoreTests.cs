namespace FunctionalBlog.Test.Identity;

public sealed class InMemoryPasswordResetTokenStoreTests : PasswordResetTokenStoreContract
{
    protected override IPasswordResetTokenStore CreateStore() => new InMemoryPasswordResetTokenStore();
}
