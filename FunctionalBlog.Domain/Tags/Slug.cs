namespace FunctionalBlog.Domain.Tags;

public static class Slug
{
    // Builds a URL slug from a tag name: lowercased, German umlauts and ß transliterated
    // (ä→ae, ö→oe, ü→ue, ß→ss) and spaces turned into hyphens — e.g. "süss" → "suess",
    // "Schweizer Küche" → "schweizer-kueche". Kept deliberately simple and mirrored by the
    // SQL in migration 0011 so DB-side and app-side slugs always agree.
    public static string From(string text) =>
        text.Trim()
            .ToLowerInvariant()
            .Replace("ä", "ae")
            .Replace("ö", "oe")
            .Replace("ü", "ue")
            .Replace("ß", "ss")
            .Replace(' ', '-');
}
