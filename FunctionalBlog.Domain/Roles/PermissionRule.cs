namespace FunctionalBlog.Domain.Roles;

public sealed record PermissionRule(string ActionName, string ResourceKey);
