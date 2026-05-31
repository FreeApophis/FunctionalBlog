namespace FunctionalBlog;

public static class NavViews
{
    public static string Nav(IPrincipal principal, Translate t)
    {
        var links = Html.Link("/", t("nav.blog")) + " · " + Html.Link("/recipes", t("nav.recipes"));

        if (principal is AuthenticatedUser user)
        {
            var settingsLink = Html.Link("/settings", t("nav.settings"));
            var logoutForm =
                """<form method="post" action="/logout" style="display:inline">""" +
                $"""<button type="submit">{Html.Encode(t("nav.logout"))}</button>""" +
                "</form>";
            var adminLink = principal.Can<Manage>(new UserResource())
                ? " · " + Html.Link("/admin/users", t("nav.admin"))
                : string.Empty;

            links += $" · {Html.Encode(user.DisplayName.Value)} · {settingsLink} · {logoutForm}{adminLink}";
        }
        else
        {
            links += " · " + Html.Link("/login", t("nav.login")) + " · " + Html.Link("/register", t("nav.register"));
        }

        links += " · " + LanguageSelector(t);

        return $"<nav>{links}</nav>";
    }

    private static string LanguageSelector(Translate t)
    {
        var options = string.Join(string.Empty, Languages.Supported.Select(lang =>
            $"<option value=\"{Html.Encode(lang)}\">{Html.Encode(Languages.Names[lang])}</option>"));

        return $"""
            <form method="post" action="/lang" class="lang-form">
                <label>{Html.Encode(t("nav.language"))}:
                    <select name="lang" onchange="this.form.submit()">{options}</select>
                </label>
            </form>
            """;
    }
}
