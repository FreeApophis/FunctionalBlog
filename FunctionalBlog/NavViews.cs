namespace FunctionalBlog;

public static class NavViews
{
    public static string Nav(ViewContext ctx)
    {
        var (principal, t, csrfToken) = ctx;
        var links = Html.Link("/", t("nav.blog")) + Html.Raw(" · ") + Html.Link("/recipes", t("nav.recipes"));

        if (principal is AuthenticatedUser user)
        {
            var settingsLink = Html.Link("/settings", t("nav.settings"));
            var logoutForm = Html.Form("/logout", Html.CsrfField(csrfToken) + Html.Button(t("nav.logout")), style: "display:inline");
            var adminLink = principal.Can<Manage>(new UserResource())
                ? Html.Raw(" · ") + Html.Link("/admin/users", t("nav.admin"))
                : HtmlString.Empty;

            links += Html.Raw(" · ") + Html.Text(user.DisplayName.Value) + Html.Raw(" · ") + settingsLink + Html.Raw(" · ") + logoutForm + adminLink;
        }
        else
        {
            links += Html.Raw(" · ") + Html.Link("/login", t("nav.login")) + Html.Raw(" · ") + Html.Link("/register", t("nav.register"));
        }

        links += Html.Raw(" · ") + LanguageSelector(t, csrfToken);
        links += Html.Raw(" ") + SearchBox(t);

        return $"<nav>{links.Render()}</nav>";
    }

    private static HtmlString SearchBox(Translate t) =>
        Html.Raw("""<form action="/search" method="get" class="search-form">""") +
        Html.Input("q") +
        Html.Button(t("nav.search")) +
        Html.Raw("</form>");

    private static HtmlString LanguageSelector(Translate t, string csrfToken)
    {
        var options = string.Join(string.Empty, Languages.Supported.Select(lang =>
            $"<option value=\"{Html.Encode(lang)}\">{Html.Encode(Languages.Names[lang])}</option>"));

        var selector = Html.Label(Html.Text(t("nav.language")) + Html.Raw($""":<select name="lang" onchange="this.form.submit()">{options}</select>"""));
        return Html.Form("/lang", Html.CsrfField(csrfToken) + selector, cssClass: "lang-form");
    }
}
