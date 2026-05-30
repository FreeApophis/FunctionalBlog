namespace FunctionalBlog.Test.Articles;

public sealed class ArticleFormTests
{
    [Fact]
    public void Valid_form_is_valid()
    {
        var form = Decode("Guter Titel", "Ein ausreichend langer Teaser.", "Genug Text hier");

        Assert.True(form.IsValid);
        Assert.Empty(form.Errors);
    }

    [Fact]
    public void Title_shorter_than_3_characters_adds_error()
    {
        var form = Decode("Ti", "Ein ausreichend langer Teaser.", "Genug Text hier");

        Assert.False(form.IsValid);
        Assert.NotEmpty(form.Errors);
    }

    [Fact]
    public void Teaser_shorter_than_10_characters_adds_error()
    {
        var form = Decode("Guter Titel", "Zu kurz", "Genug Text hier");

        Assert.False(form.IsValid);
        Assert.NotEmpty(form.Errors);
    }

    [Fact]
    public void Text_shorter_than_10_characters_adds_error()
    {
        var form = Decode("Guter Titel", "Ein ausreichend langer Teaser.", "Zu kurz");

        Assert.False(form.IsValid);
        Assert.NotEmpty(form.Errors);
    }

    private static DecodedArticleForm Decode(string title, string teaser, string text) =>
        ArticleForm.Decode(new Request(
            "POST",
            "/articles",
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string> { ["title"] = title, ["teaser"] = teaser, ["text"] = text },
            new Dictionary<string, string>()));
}
