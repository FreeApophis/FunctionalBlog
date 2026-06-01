namespace FunctionalBlog.Domain.Roles;

public sealed record IngredientResource : IResource
{
    public string ResourceKey => "ingredient";
}
