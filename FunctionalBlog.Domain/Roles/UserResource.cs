namespace FunctionalBlog.Domain.Roles;

public sealed record UserResource : IResource
{
    public string ResourceKey => "user";
}
