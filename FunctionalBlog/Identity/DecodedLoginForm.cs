namespace FunctionalBlog.Identity;

public sealed record DecodedLoginForm(
    bool IsValid,
    IReadOnlyList<string> Errors,
    string EmailRaw,
    string Password);
