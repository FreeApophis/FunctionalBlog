namespace FunctionalBlog.Test.Images;

public sealed class InMemoryImageRepositoryTests : ImageRepositoryContract
{
    protected override IImageRepository CreateRepository() => new InMemoryImageRepository();
}
