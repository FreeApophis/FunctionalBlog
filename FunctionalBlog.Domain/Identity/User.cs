namespace FunctionalBlog.Domain.Identity;

public sealed record User(
    UserId Id,
    Email Email,
    string PasswordHash,
    IReadOnlyList<string> RoleNames,
    DateTimeOffset CreatedAt)
{
    public static User Create(
        UserId id,
        Email email,
        string passwordHash,
        IReadOnlyList<string> roleNames,
        DateTimeOffset createdAt) =>
        new(id, email, passwordHash, roleNames, createdAt);
}
