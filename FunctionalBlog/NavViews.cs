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

        var chips = Html.Link("/recipes", t("nav.recipes")) +
            (principal is AuthenticatedUser ? Html.Link("/users", t("nav.users")) : HtmlString.Empty);

        var actions = SearchBox(t) +
            LanguageSelector(t, csrfToken, ctx.Language) +
            ThemeToggle(ctx.Theme, t, csrfToken) +
            UserMenu(ctx);

        return $"""
            <header class="masthead"><div class="masthead-inner">
                <a class="brand" href="/">
                    <img class="brand-banner" src="/assets/foodblog-banner.png" alt="Foodblog" />
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

    // Circular trigger (same footprint as the theme toggle) revealing a hover/focus dropdown.
    // Authenticated: initial avatar → Settings, Admin (when permitted), Logout. Guest: person
    // icon → Login, Register.
    private static HtmlString UserMenu(ViewContext ctx)
    {
        var (principal, t, csrfToken) = ctx;

        if (principal is AuthenticatedUser user)
        {
            var name = user.DisplayName.Value;
            var initial = name.Length > 0 ? name[..1].ToUpperInvariant() : "?";

            var adminItem = AdminDashboardViews.HasAnyAccess(principal)
                ? $"""<a href="/admin" role="menuitem">{Html.Encode(t("nav.admin"))}</a>"""
                : string.Empty;

            return Html.Raw($"""
                <div class="user-menu">
                    <button type="button" class="user-trigger is-auth" aria-haspopup="true" aria-label="{Html.Encode(name)}">{Html.Encode(initial)}</button>
                    <div class="user-dropdown"><div class="user-panel">
                        <div class="user-panel-name">{Html.Encode(name)}</div>
                        <a href="/settings" role="menuitem">{Html.Encode(t("nav.settings"))}</a>
                        {adminItem}
                        <form method="post" action="/logout" style="margin:0;display:block">
                            {Html.CsrfField(csrfToken).Render()}
                            <button type="submit" role="menuitem">{Html.Encode(t("nav.logout"))}</button>
                        </form>
                    </div></div>
                </div>
                """);
        }

        return Html.Raw($"""
            <div class="user-menu">
                <button type="button" class="user-trigger" aria-haspopup="true" aria-label="{Html.Encode(t("nav.account"))}">{PersonIcon}</button>
                <div class="user-dropdown"><div class="user-panel">
                    <a href="/login" role="menuitem">{Html.Encode(t("nav.login"))}</a>
                    <a href="/register" role="menuitem">{Html.Encode(t("nav.register"))}</a>
                </div></div>
            </div>
            """);
    }

    private static HtmlString SearchBox(Translate t) =>
        Html.Raw($"""
            <form action="/search" method="get" class="search-form" role="search">
                <input name="q" placeholder="{Html.Encode(t("nav.search"))}" aria-label="{Html.Encode(t("nav.search"))}" />
                <button type="submit" aria-label="{Html.Encode(t("nav.search"))}">{SearchIcon}</button>
            </form>
            """);

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

    private const string SearchIcon =
        """<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="7"/><path d="m21 21-4.3-4.3"/></svg>""";

    private const string PersonIcon =
        """<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="4"/><path d="M4 21v-1a6 6 0 0 1 6-6h4a6 6 0 0 1 6 6v1"/></svg>""";

    private const string MoonIcon =
        """<svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round"><path d="M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8Z"/></svg>""";

    private const string SunIcon =
        """<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round"><circle cx="12" cy="12" r="4"/><path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4"/></svg>""";
}
