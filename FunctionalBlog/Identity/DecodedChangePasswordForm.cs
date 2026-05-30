namespace FunctionalBlog.Identity;

public sealed record DecodedChangePasswordForm(
    bool IsValid,
    IReadOnlyList<string> Errors,
    string CurrentPassword,
    string NewPassword);
