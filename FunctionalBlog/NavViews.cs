namespace FunctionalBlog;

public static class NavViews
{
    public static string Nav(IPrincipal principal, Translate t)
    {
        var links = Html.Link("/", t("nav.blog")) + " · " + Html.Link("/recipes", t("nav.recipes"));

        if (principal is AuthenticatedUser user)
        {
            var settingsLink = Html.Link("/settings", t("nav.settings"));
            var logoutForm = Html.Form("/logout", Html.Button(t("nav.logout")), style: "display:inline");
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

        var selector = Html.Label(Html.Encode(t("nav.language")) + $""":<select name="lang" onchange="this.form.submit()">{options}</select>""");
        return Html.Form("/lang", selector, cssClass: "lang-form");
    }
}
