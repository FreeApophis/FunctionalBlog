namespace FunctionalBlog.Search;

public static class SearchViews
{
    public static string Empty(ViewContext ctx)
    {
        var (_, t, _) = ctx;
        var body =
            Html.H1(t("search.title")) +
            SearchForm(string.Empty, t) +
            Html.P(Html.Text(t("search.prompt")));

        return Layout.Page(t("search.title"), body, ctx);
    }

    public static string Results(
        string query,
        IReadOnlyList<SearchResult> results,
        IReadOnlyList<string> suggestions,
        ViewContext ctx)
    {
        var (_, t, _) = ctx;
        var form = SearchForm(query, t);

        var header = results.Count == 0
            ? Html.P(Html.Text(t("search.no_results")))
            : Html.P(Html.Text($"{results.Count} {t("search.results_for")} „{query}“"));

        var suggestionHtml = suggestions.Count > 0
            ? Html.P(Html.Text(t("search.did_you_mean") + " ") +
                HtmlString.Concat(suggestions.Select((s, i) =>
                    (i > 0 ? Html.Raw(", ") : HtmlString.Empty) +
                    Html.Link($"/search?q={Uri.EscapeDataString(s)}", s))))
            : HtmlString.Empty;

        var resultList = results.Count == 0
            ? HtmlString.Empty
            : Html.Ul(results.Select(r => ResultItem(r, t)));

        var body = Html.H1(t("search.title")) + form + header + suggestionHtml + resultList;

        return Layout.Page(t("search.title"), body, ctx);
    }

    private static HtmlString SearchForm(string query, Translate t) =>
        Html.Raw($"""<form action="/search" method="get" class="search-form">""") +
        Html.Input("q", query) +
        Html.Button(t("search.submit")) +
        Html.Raw("</form>");

    private static HtmlString ResultItem(SearchResult result, Translate t)
    {
        var typeBadge = result.Type switch
        {
            "article" => t("search.type.article"),
            "recipe" => t("search.type.recipe"),
            "ingredient" => t("search.type.ingredient"),
            _ => result.Type,
        };

        var link = result.Type switch
        {
            "article" => $"/articles/{result.Id}",
            "recipe" => $"/recipes/{result.Id}",
            _ => null,
        };

        var titleHtml = link is not null
            ? Html.Link(link, result.Title)
            : Html.Text(result.Title);

        var snippet = string.IsNullOrEmpty(result.Snippet)
            ? HtmlString.Empty
            : Html.Raw($"<p class=\"search-snippet\">{result.Snippet}</p>");

        return Html.Raw($"<span class=\"search-badge\">{Html.Encode(typeBadge)}</span> ") +
            titleHtml +
            snippet;
    }
}
