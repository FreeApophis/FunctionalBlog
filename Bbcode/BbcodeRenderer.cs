using System.Net;
using System.Text.RegularExpressions;

namespace Bbcode;

// A deliberately tiny, XSS-safe BBCode renderer. The whole input is HTML-encoded *first*
// (so all literal text, quotes and angle brackets are neutralised) and only then is a fixed
// whitelist of tags transformed into HTML. Because `[` and `]` survive encoding but `< > " &`
// do not, attribute break-outs and stray markup are impossible by construction; URLs get an
// extra scheme allowlist so `javascript:`/`data:` never become links or image sources.
//
// The result is a trusted HTML fragment string. It has no dependency on the host application,
// so callers are responsible for treating the output as already-safe markup.
public static partial class BbcodeRenderer
{
    public static string RenderToHtml(string text)
    {
        var normalized = (text ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");
        var encoded = WebUtility.HtmlEncode(normalized);

        var paragraphs = ParagraphBreak()
            .Split(encoded)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => "<p>" + RenderInline(p) + "</p>");

        return string.Concat(paragraphs);
    }

    private static string RenderInline(string paragraph)
    {
        paragraph = ImgTag().Replace(paragraph, RenderImg);
        paragraph = UrlTag().Replace(paragraph, RenderUrl);
        paragraph = BoldTag().Replace(paragraph, "<strong>$1</strong>");
        paragraph = ItalicTag().Replace(paragraph, "<em>$1</em>");
        return paragraph.Replace("\n", "<br />");
    }

    private static string RenderImg(Match match)
    {
        var url = match.Groups[1].Value.Trim();
        return IsSafeUrl(url)
            ? $"""<img src="{url}" alt="" loading="lazy" />"""
            : match.Value;
    }

    private static string RenderUrl(Match match)
    {
        var href = match.Groups[1].Value.Trim();
        var inner = match.Groups[2].Value;
        return IsSafeUrl(href)
            ? $"""<a href="{href}">{inner}</a>"""
            : match.Value;
    }

    // The url is already HTML-encoded, so it cannot contain a raw quote or angle bracket.
    private static bool IsSafeUrl(string url) =>
        url.StartsWith('/')
        || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(@"\n[ \t]*\n")]
    private static partial Regex ParagraphBreak();

    [GeneratedRegex(@"\[b\](.*?)\[/b\]", RegexOptions.Singleline)]
    private static partial Regex BoldTag();

    [GeneratedRegex(@"\[i\](.*?)\[/i\]", RegexOptions.Singleline)]
    private static partial Regex ItalicTag();

    [GeneratedRegex(@"\[img\](.*?)\[/img\]", RegexOptions.Singleline)]
    private static partial Regex ImgTag();

    [GeneratedRegex(@"\[url=(.*?)\](.*?)\[/url\]", RegexOptions.Singleline)]
    private static partial Regex UrlTag();
}
