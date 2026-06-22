namespace FunctionalBlog;

public static class NavViews
{
    // Dark brand strip above the masthead.
    public static string UtilityBar() =>
        """
        <div class="utility-bar"><div class="utility-inner">
            <span>FOODBLOG.CH — KOCHEN · BACKEN · GENIESSEN</span>
            <span class="utility-meta"><span>SEIT 2014</span><span class="dot">·</span><span>ZÜRICH</span></span>
        </div></div>
        """;

    public static string Masthead(ViewContext ctx)
    {
        var (principal, t, csrfToken) = ctx;

        var chips = Html.Link("/", t("nav.blog")) +
            Html.Link("/recipes", t("nav.recipes")) +
            Html.Link("/pages", t("nav.pages")) +
            (principal is AuthenticatedUser ? Html.Link("/users", t("nav.users")) : HtmlString.Empty) +
            (principal.Can<Manage>(new ImageResource()) ? Html.Link("/images", t("nav.images")) : HtmlString.Empty) +
            (principal.Can<Manage>(new UserResource()) ? Html.Link("/admin/users", t("nav.admin")) : HtmlString.Empty);

        HtmlString account = principal is AuthenticatedUser user
            ? Html.Raw($"""<span class="mast-user">{Html.Encode(user.DisplayName.Value)}</span>""") +
                Html.Link("/settings", t("nav.settings")) +
                Html.Form("/logout", Html.CsrfField(csrfToken) + Html.Button(t("nav.logout")), style: "display:inline")
            : Html.Link("/login", t("nav.login")) + Html.Link("/register", t("nav.register"));

        var actions = account +
            LanguageSelector(t, csrfToken, ctx.Language) +
            SearchBox(t) +
            ThemeToggle(ctx.Theme, t, csrfToken);

        return $"""
            <header class="masthead"><div class="masthead-inner">
                <a class="brand" href="/">
                    <span class="brand-logo">{BrandLogo}</span>
                    <span class="brand-text"><span class="brand-name">Foodblog</span><span class="brand-tag">SELBER MACHEN</span></span>
                </a>
                <nav class="mast-nav">{chips.Render()}</nav>
                <div class="mast-actions">{actions.Render()}</div>
            </div></header>
            """;
    }

    public static string Footer(ViewContext ctx)
    {
        var (_, t, _) = ctx;

        string Item(string href, string label) => $"<li>{Html.Link(href, label).Render()}</li>";

        var links = Item("/", t("nav.blog")) + Item("/recipes", t("nav.recipes")) + Item("/pages", t("nav.pages"));

        return $"""
            <footer class="site-footer">
                <div class="footer-inner">
                    <div>
                        <div class="footer-brand">Foodblog</div>
                        <p class="footer-blurb">{Html.Encode(t("footer.blurb"))}</p>
                    </div>
                    <div>
                        <h5 class="footer-head">{Html.Encode(t("footer.section_site"))}</h5>
                        <ul class="footer-links">{links}</ul>
                    </div>
                </div>
                <div class="footer-bar"><div class="footer-bar-inner">
                    <span>© 2026 FOODBLOG.CH</span><span>MADE WITH ❤ IN ZÜRICH</span>
                </div></div>
            </footer>
            """;
    }

    private static HtmlString SearchBox(Translate t) =>
        Html.Raw("""<form action="/search" method="get" class="search-form">""") +
        Html.Input("q") +
        Html.Button(t("nav.search")) +
        Html.Raw("</form>");

    private static HtmlString LanguageSelector(Translate t, string csrfToken, string current)
    {
        var options = string.Join(string.Empty, Languages.Supported.Select(lang =>
            $"<option value=\"{Html.Encode(lang)}\"{(lang == current ? " selected" : string.Empty)}>{Html.Encode(Languages.Names[lang])}</option>"));

        var selector = Html.Label(Html.Text(t("nav.language")) + Html.Raw($""":<select name="lang" onchange="this.form.submit()">{options}</select>"""));
        return Html.Form("/lang", Html.CsrfField(csrfToken) + selector, cssClass: "lang-form");
    }

    private static HtmlString ThemeToggle(string current, Translate t, string csrfToken)
    {
        var next = current == "dark" ? "light" : "dark";
        var title = current == "dark" ? t("nav.theme_light") : t("nav.theme_dark");
        var icon = current == "dark" ? SunIcon : MoonIcon;

        var body = Html.CsrfField(csrfToken) +
            Html.Raw($"""<input type="hidden" name="theme" value="{Html.Encode(next)}" />""") +
            Html.Raw($"""<button type="submit" class="theme-toggle" title="{Html.Encode(title)}" aria-label="{Html.Encode(title)}">{icon}</button>""");

        return Html.Form("/theme", body, cssClass: "theme-form");
    }

    private const string BrandLogo =
        """<svg width="22" height="22" viewBox="0 0 24 24" fill="none"><path d="M12 3c-2.2 0-4 1.8-4 4 0 1.3.6 2.4 1.5 3.2L8 21h8l-1.5-10.8C15.4 9.4 16 8.3 16 7c0-2.2-1.8-4-4-4Z" fill="#fffefb"/><circle cx="12" cy="7" r="1.3" fill="#5b7b53"/></svg>""";

    private const string MoonIcon =
        """<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round"><path d="M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8Z"/></svg>""";

    private const string SunIcon =
        """<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round"><circle cx="12" cy="12" r="4"/><path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4"/></svg>""";
}
