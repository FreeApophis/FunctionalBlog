using System.Net;

public static class Html
{
    public static string Encode(string value) => WebUtility.HtmlEncode(value);

    public static string H1(string value) => $"<h1>{Encode(value)}</h1>";

    public static string H2(string value) => $"<h2>{value}</h2>";

    public static string P(string value) => $"<p>{value}</p>";

    public static string Small(string value) => $"<small>{Encode(value)}</small>";

    public static string Link(string href, string text) => $"<a href=\"{Encode(href)}\">{Encode(text)}</a>";

    public static string Article(string body) => $"<article>{body}</article>";

    public static string Div(string cssClass, string body) => $"<div class=\"{Encode(cssClass)}\">{body}</div>";

    public static string Ul(IEnumerable<string> encodedItems) =>
        "<ul>" + string.Join(string.Empty, encodedItems.Select(x => $"<li>{x}</li>")) + "</ul>";

    public static string Paragraphs(string text) =>
        string.Join(string.Empty, text
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => P(Encode(x))));
}
