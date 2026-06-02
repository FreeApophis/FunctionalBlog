namespace FunctionalBlog.Test;

public class LayoutTests
{
    [Fact]
    public void Page_does_not_inline_styles()
    {
        var html = Layout.Page("Titel", Html.Raw("<p>Inhalt</p>"));

        Assert.DoesNotContain("<style", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" style=\"", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Page_links_to_external_stylesheet()
    {
        var html = Layout.Page("Titel", Html.Raw("<p>Inhalt</p>"));

        Assert.Contains("<link", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rel=\"stylesheet\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("href=\"/styles.css\"", html, StringComparison.OrdinalIgnoreCase);
    }
}
