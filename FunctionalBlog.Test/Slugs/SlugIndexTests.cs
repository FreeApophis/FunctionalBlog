namespace FunctionalBlog.Test.Slugs;

public class SlugIndexTests
{
    [Fact]
    public void Url_uses_the_registered_slug_when_present()
    {
        var index = new SlugIndex(new Dictionary<string, IReadOnlyDictionary<int, string>>
        {
            [SlugEntityType.Recipe] = new Dictionary<int, string> { [7] = "ruehrkuchen" },
        });

        Assert.Equal("/recipes/ruehrkuchen", index.Url(SlugEntityType.Recipe, 7));
        Assert.Equal("ruehrkuchen", index.For(SlugEntityType.Recipe, 7));
    }

    [Fact]
    public void Url_falls_back_to_the_numeric_id_when_no_slug_is_registered()
    {
        Assert.Equal("/recipes/7", SlugIndex.Empty.Url(SlugEntityType.Recipe, 7));
        Assert.Equal("/articles/3", SlugIndex.Empty.Url(SlugEntityType.Article, 3));
        Assert.Equal("/pages/4", SlugIndex.Empty.Url(SlugEntityType.Page, 4));
        Assert.Equal("/ingredients/9", SlugIndex.Empty.Url(SlugEntityType.Ingredient, 9));
        Assert.Equal("9", SlugIndex.Empty.For(SlugEntityType.Ingredient, 9));
    }
}
