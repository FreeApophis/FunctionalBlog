namespace FunctionalBlog.Domain.Roles;

public sealed record Role(RoleId Id, string Name, IReadOnlyList<PermissionRule> Rules)
{
    public static Role Create(RoleId id, string name) =>
        new(id, name, Array.Empty<PermissionRule>());

    public Role AddRule(PermissionRule rule) =>
        this with { Rules = [..Rules, rule] };

    public Role RemoveRule(PermissionRule rule) =>
        this with { Rules = Rules.Where(r => r != rule).ToList() };
}
