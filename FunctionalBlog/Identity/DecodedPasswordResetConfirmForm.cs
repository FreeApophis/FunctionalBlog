namespace FunctionalBlog.Identity;

public sealed record DecodedPasswordResetConfirmForm(
    bool IsValid,
    IReadOnlyList<string> Errors,
    string Token,
    string Password);
