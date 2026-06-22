namespace FunctionalBlog.Admin;

public static class AdminDashboardViews
{
    private const string UsersIcon =
        """<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75"/></svg>""";

    private const string RolesIcon =
        """<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10Z"/><path d="m9 12 2 2 4-4"/></svg>""";

    private const string UnitsIcon =
        """<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M3 6h18M3 12h18M3 18h18"/><path d="M7 3v3M12 9v3M17 15v3"/></svg>""";

    private const string IngredientsIcon =
        """<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M12 2a7 7 0 0 0-7 7c0 3 2 5 2 8a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2c0-3 2-5 2-8a7 7 0 0 0-7-7Z"/><path d="M9 21h6"/></svg>""";

    private const string ImagesIcon =
        """<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="9" cy="9" r="2"/><path d="m21 15-3.5-3.5L8 21"/></svg>""";

    private const string PagesIcon =
        """<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8Z"/><path d="M14 2v6h6M9 13h6M9 17h6"/></svg>""";

    private const string BlogIcon =
        """<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2Z"/></svg>""";

    private const string TranslationsIcon =
        """<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="m5 8 6 6M4 14l6-6 2-3M2 5h12M7 2h1M22 22l-5-10-5 10M14 18h6"/></svg>""";

    private static readonly IReadOnlyList<CardDef> AllCards =
    [
        new("/admin/users", "admin.dashboard.card.users", "admin.dashboard.card.users_desc", UsersIcon, p => p.Can<Manage>(new UserResource())),
        new("/admin/roles", "admin.dashboard.card.roles", "admin.dashboard.card.roles_desc", RolesIcon, p => p.Can<Manage>(new RoleResource())),
        new("/admin/units", "admin.dashboard.card.units", "admin.dashboard.card.units_desc", UnitsIcon, p => p.Can<Manage>(new UnitResource())),
        new("/admin/ingredients", "admin.dashboard.card.ingredients", "admin.dashboard.card.ingredients_desc", IngredientsIcon, p => p.Can<Manage>(new IngredientResource())),
        new("/images", "admin.dashboard.card.images", "admin.dashboard.card.images_desc", ImagesIcon, p => p.Can<Manage>(new ImageResource())),
        new("/pages", "admin.dashboard.card.pages", "admin.dashboard.card.pages_desc", PagesIcon, p => p.Can<Create>(new PageResource())),
        new("/", "admin.dashboard.card.blog", "admin.dashboard.card.blog_desc", BlogIcon, p => p.Can<Create>(new ArticleResource())),
        new("/admin/translations", "admin.dashboard.card.translations", "admin.dashboard.card.translations_desc", TranslationsIcon, p => p.Can<Manage>(new UserResource())),
    ];

    // The admin landing page: a grid of cards, one per manageable section. Each card is only
    // rendered when the current principal actually has permission to reach its target route,
    // so the dashboard never advertises a section the user would get a 403 on.
    public static string Dashboard(ViewContext ctx)
    {
        var (principal, t, _) = ctx;

        var visible = AllCards.Where(c => c.IsVisible(principal)).ToList();

        var grid = visible.Count > 0
            ? Html.Raw($"""<div class="admin-grid">{string.Concat(visible.Select(c => Card(c, t)))}</div>""")
            : Html.P(Html.Text(t("admin.dashboard.empty")));

        var head = Html.Raw($"""
            <div class="page-head">
                <div class="eyebrow eyebrow-accent">{Html.Encode(t("admin.dashboard.eyebrow"))}</div>
                <div class="page-head-row"><h1>{Html.Encode(t("admin.dashboard.title"))}</h1></div>
                <p class="page-head-blurb">{Html.Encode(t("admin.dashboard.blurb"))}</p>
            </div>
            """);

        return Layout.Page(t("admin.dashboard.title"), head + grid, ctx);
    }

    // True when the principal can reach at least one admin section — used by the nav to decide
    // whether to surface the "Admin" link at all.
    public static bool HasAnyAccess(IPrincipal principal) =>
        AllCards.Any(c => c.IsVisible(principal));

    private static HtmlString Card(CardDef def, Translate t) =>
        Html.Raw($"""
            <a class="admin-card" href="{Html.Encode(def.Href)}">
                <span class="admin-card-icon">{def.Icon}</span>
                <span class="admin-card-body">
                    <span class="admin-card-title">{Html.Encode(t(def.TitleKey))}</span>
                    <span class="admin-card-desc">{Html.Encode(t(def.DescKey))}</span>
                </span>
            </a>
            """);

    private sealed record CardDef(string Href, string TitleKey, string DescKey, string Icon, Func<IPrincipal, bool> IsVisible);
}
