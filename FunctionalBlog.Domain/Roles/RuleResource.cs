namespace FunctionalBlog.Domain.Roles;

public sealed record RuleResource : IResource
{
    public string ResourceKey => "rule";
}
