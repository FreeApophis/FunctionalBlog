namespace FunctionalBlog.Identity;

public sealed record DecodedRegisterForm(
    bool IsValid,
    IReadOnlyList<string> Errors,
    Option<Email> Email,
    string DisplayName,
    string Password);
