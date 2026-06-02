namespace FunctionalBlog.Translations;

public static class TranslationViews
{
    public static string List(IReadOnlyList<Translation> translations, IPrincipal principal, Translate t)
    {
        var grouped = translations
            .GroupBy(tr => tr.Key)
            .OrderBy(g => g.Key)
            .ToList();

        var langHeaders = string.Concat(Languages.Supported.Select(lang => Html.Th(lang.ToUpperInvariant())));

        string Row(IGrouping<string, Translation> group)
        {
            var cells = string.Concat(Languages.Supported.Select(lang =>
            {
                var entry = group.FirstOrDefault(tr => tr.Language == lang && tr.Variant == null);
                var text = entry?.Text ?? string.Empty;
                var form = Html.Form(
                    $"/admin/translations/{Uri.EscapeDataString(group.Key)}/{lang}",
                    Html.Input("text", text, style: "width:100%") + Html.Button(t("translations.save")));
                return Html.Td(form);
            }));
            return Html.Tr(Html.Td($"<code>{Html.Encode(group.Key)}</code>") + cells);
        }

        var rows = grouped.Count == 0
            ? Html.Tr(Html.Td("–", colspan: 5))
            : string.Concat(grouped.Select(Row));

        var table = Html.Table(
            Html.Thead(Html.Tr(Html.Th(t("translations.key")) + langHeaders)) +
            Html.Tbody(rows));

        var body = Html.H1(t("translations.title")) +
            Html.P(Html.Link("/admin/users", "← Admin")) +
            Html.P(Html.Link("/admin/translations/export.json", t("translations.export"))) +
            table;

        return Layout.Page(t("translations.title"), body, principal, t);
    }
}
