namespace FunctionalBlog.Identity;

public sealed record DecodedRegisterForm(
    bool IsValid,
    IReadOnlyList<string> Errors,
    Email? Email,
    string Password);
