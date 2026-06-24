namespace FunctionalBlog.Pages;

// Builds share/SEO metadata for a static content page: Open Graph values and a meta description
// drawn from the page body. Generic pages get no structured data — just a clean share card.
public static class PageSeo
{
    public static PageMeta Build(Page page, string baseUrl) =>
        new()
        {
            Type = "website",
            Url = $"{baseUrl}/pages/{page.Id.Value}",
            Description = Seo.PlainTextSnippet(page.Content.Value),
        };
}
