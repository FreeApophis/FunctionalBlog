namespace FunctionalBlog.Test.Articles;

public class ArticleTests
{
    [Fact]
    public void Create_returns_an_article_carrying_the_supplied_values()
    {
        var id = new ArticleId(42);
        var title = new ArticleTitle("Hallo Welt");
        var text = new ArticleText("Erster Beitrag.");
        var createdAt = new DateTimeOffset(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

        var article = Article.Create(id, title, text, createdAt);

        Assert.Equal(new Article(id, title, text, createdAt), article);
    }
}
