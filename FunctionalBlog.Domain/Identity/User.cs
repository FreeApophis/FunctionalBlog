namespace FunctionalBlog.Domain.Identity;

public sealed record User(
    UserId Id,
    Email Email,
    DisplayName DisplayName,
    string PasswordHash,
    IReadOnlyList<string> RoleNames,
    DateTimeOffset CreatedAt)
{
    public static User Create(
        UserId id,
        Email email,
        DisplayName displayName,
        string passwordHash,
        IReadOnlyList<string> roleNames,
        DateTimeOffset createdAt) =>
        new(id, email, displayName, passwordHash, roleNames, createdAt);
}
