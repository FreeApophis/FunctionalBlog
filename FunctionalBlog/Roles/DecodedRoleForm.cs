namespace FunctionalBlog.Roles;

public sealed record DecodedRoleForm(
    bool IsValid,
    IReadOnlyList<string> Errors,
    string Name);
