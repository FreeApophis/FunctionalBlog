namespace FunctionalBlog.Domain.Roles;

public sealed record RoleResource : IResource
{
    public string ResourceKey => "role";
}
