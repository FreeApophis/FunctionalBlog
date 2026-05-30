namespace FunctionalBlog.Translations;

public static class TranslationViews
{
    public static string List(IReadOnlyList<Translation> translations, IPrincipal principal, Translate t)
    {
        var grouped = translations
            .GroupBy(tr => tr.Key)
            .OrderBy(g => g.Key)
            .ToList();

        var langHeaders = string.Join(string.Empty, Languages.Supported.Select(
            lang => $"<th>{Html.Encode(lang.ToUpperInvariant())}</th>"));

        string Row(IGrouping<string, Translation> group)
        {
            var cells = string.Join(string.Empty, Languages.Supported.Select(lang =>
            {
                var entry = group.FirstOrDefault(tr => tr.Language == lang && tr.Variant == null);
                var text = entry?.Text ?? string.Empty;
                return $"""
                    <td>
                        <form method="post" action="/admin/translations/{Uri.EscapeDataString(group.Key)}/{Html.Encode(lang)}">
                            <input name="text" value="{Html.Encode(text)}" style="width:100%" />
                            <button type="submit">{t("translations.save")}</button>
                        </form>
                    </td>
                    """;
            }));
            return $"<tr><td><code>{Html.Encode(group.Key)}</code></td>{cells}</tr>";
        }

        var rows = grouped.Count == 0
            ? "<tr><td colspan=\"5\">–</td></tr>"
            : string.Join(string.Empty, grouped.Select(Row));

        var table = $"""
            <table>
                <thead><tr><th>{t("translations.key")}</th>{langHeaders}</tr></thead>
                <tbody>{rows}</tbody>
            </table>
            """;

        var body = Html.H1(t("translations.title")) +
            Html.P(Html.Link("/admin/users", "← Admin")) +
            Html.P(Html.Link("/admin/translations/export.json", t("translations.export"))) +
            table;

        return Layout.Page(t("translations.title"), body, principal, t);
    }
}
