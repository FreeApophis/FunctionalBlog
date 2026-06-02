using System.Net;

namespace FunctionalBlog;

public static class Html
{
    public static string Encode(string value) => WebUtility.HtmlEncode(value);

    public static string H1(string value) => $"<h1>{Encode(value)}</h1>";

    public static string H2(string value) => $"<h2>{Encode(value)}</h2>";

    public static string P(string value) => $"<p>{value}</p>";

    public static string Small(string value) => $"<small>{Encode(value)}</small>";

    public static string Link(string href, string text) => $"<a href=\"{Encode(href)}\">{Encode(text)}</a>";

    public static string Article(string body) => $"<article>{body}</article>";

    public static string Div(string cssClass, string body) => $"<div class=\"{Encode(cssClass)}\">{body}</div>";

    public static string Form(string action, string body, string? cssClass = null, string? style = null)
    {
        var classAttr = cssClass is null ? string.Empty : $" class=\"{Encode(cssClass)}\"";
        var styleAttr = style is null ? string.Empty : $" style=\"{Encode(style)}\"";
        return $"""<form method="post" action="{Encode(action)}"{classAttr}{styleAttr}>{body}</form>""";
    }

    public static string Button(string text) => $"<button type=\"submit\">{Encode(text)}</button>";

    public static string Fieldset(string legend, string body) =>
        $"""<fieldset><legend>{Encode(legend)}</legend>{body}</fieldset>""";

    public static string Label(string body) => $"<label>{body}</label>";

    public static string Input(string name, string value = "", string? style = null)
    {
        var styleAttr = style is null ? string.Empty : $" style=\"{Encode(style)}\"";
        return $"""<input name="{Encode(name)}" value="{Encode(value)}"{styleAttr} />""";
    }

    public static string InputNumber(string name, string value, string min = "0", string? step = null)
    {
        var stepAttr = step is null ? string.Empty : $" step=\"{Encode(step)}\"";
        return $"""<input name="{Encode(name)}" type="number"{stepAttr} min="{Encode(min)}" value="{Encode(value)}" />""";
    }

    public static string InputEmail(string name, string value = "") =>
        $"""<input type="email" name="{Encode(name)}" value="{Encode(value)}" />""";

    public static string InputPassword(string name) =>
        $"""<input type="password" name="{Encode(name)}" />""";

    public static string InputHidden(string name, string value) =>
        $"""<input type="hidden" name="{Encode(name)}" value="{Encode(value)}" />""";

    public static string InputCheckbox(string name, string value, bool isChecked) =>
        $"""<input type="checkbox" name="{Encode(name)}" value="{Encode(value)}"{(isChecked ? " checked" : string.Empty)} />""";

    public static string Table(string body) => $"<table>{body}</table>";

    public static string Thead(string body) => $"<thead>{body}</thead>";

    public static string Tbody(string body) => $"<tbody>{body}</tbody>";

    public static string Tr(string body) => $"<tr>{body}</tr>";

    public static string Th(string body) => $"<th>{Encode(body)}</th>";

    public static string Td(string body, int colspan = 1) =>
        colspan > 1 ? $"""<td colspan="{colspan}">{body}</td>""" : $"<td>{body}</td>";

    public static string Ul(IEnumerable<string> items) =>
        "<ul>" + string.Join(string.Empty, items.Select(x => $"<li>{x}</li>")) + "</ul>";

    public static string Ol(IEnumerable<string> items) =>
        "<ol>" + string.Join(string.Empty, items.Select(x => $"<li>{x}</li>")) + "</ol>";

    public static string Paragraphs(string text) =>
        string.Join(string.Empty, text
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => P(Encode(x))));
}
