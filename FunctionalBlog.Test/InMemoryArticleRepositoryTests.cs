namespace FunctionalBlog.Test;

public class InMemoryArticleRepositoryTests : ArticleRepositoryContract
{
    protected override IArticleRepository CreateRepository() => new InMemoryArticleRepository();
}
