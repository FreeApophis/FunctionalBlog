namespace FunctionalBlog.Test.Tags;

public sealed class SlugTests
{
    [Theory]
    [InlineData("süss", "suess")]
    [InlineData("Süss", "suess")]
    [InlineData("Vegetarisch", "vegetarisch")]
    [InlineData("Schweizer Küche", "schweizer-kueche")]
    [InlineData("One-Pot", "one-pot")]
    [InlineData("Größe", "groesse")]
    [InlineData("  süss  ", "suess")]
    public void Transliterates_umlauts_lowercases_and_hyphenates(string input, string expected) =>
        Assert.Equal(expected, Slug.From(input));
}
