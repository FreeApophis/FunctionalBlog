namespace FunctionalBlog.Domain.Roles;

public sealed record ImageResource : IResource
{
    public string ResourceKey => "image";
}
