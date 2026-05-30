namespace FunctionalBlog.Domain.Identity;

public sealed record PasswordResetToken(string Token, UserId UserId, DateTimeOffset ExpiresAt, bool Consumed);
