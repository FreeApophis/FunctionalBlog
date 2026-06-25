namespace FunctionalBlog.Domain.Identity;

public sealed record User(
    UserId Id,
    Email Email,
    DisplayName DisplayName,
    string PasswordHash,
    IReadOnlyList<string> RoleNames,
    DateTimeOffset CreatedAt,
    Option<ImageId> AvatarImageId = default)
{
    public static User Create(
        UserId id,
        Email email,
        DisplayName displayName,
        string passwordHash,
        IReadOnlyList<string> roleNames,
        DateTimeOffset createdAt,
        Option<ImageId> avatarImageId = default) =>
        new(id, email, displayName, passwordHash, roleNames, createdAt, avatarImageId);

    public bool Equals(User? other) =>
        other is not null &&
        Id == other.Id &&
        Email == other.Email &&
        DisplayName == other.DisplayName &&
        PasswordHash == other.PasswordHash &&
        RoleNames.SequenceEqual(other.RoleNames) &&
        CreatedAt == other.CreatedAt &&
        AvatarImageId == other.AvatarImageId;

    public override int GetHashCode() => Id.GetHashCode();
}
