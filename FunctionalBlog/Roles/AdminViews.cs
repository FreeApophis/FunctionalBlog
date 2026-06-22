namespace FunctionalBlog.Roles;

public static class AdminViews
{
    private const string PencilIcon =
        """<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M12 20h9M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4Z"/></svg>""";

    private static readonly string[] AllActions = ["View", "Create", "Edit", "Delete", "Manage"];
    private static readonly string[] AllResources = ["article", "user", "role", "rule"];

    // Read-only roster styled like the units admin: a single card with a header row and one
    // line per user (name, e-mail, roles). The pencil opens the per-user role editor. Section
    // navigation lives on the /admin dashboard, not here.
    public static string UserList(IReadOnlyList<User> users, ViewContext ctx)
    {
        var (_, t, _) = ctx;

        var head = $"""
            <div class="admin-user-head">
                <span>{Html.Encode(t("admin.users.col_user"))}</span>
                <span>{Html.Encode(t("admin.users.col_email"))}</span>
                <span>{Html.Encode(t("admin.users.col_roles"))}</span>
                <span></span>
            </div>
            """;

        var rows = users.Count > 0
            ? string.Concat(users.Select(u => UserRow(u, t)))
            : $"""<p class="admin-user-empty">{Html.Encode(t("admin.users.empty"))}</p>""";

        var section = $"""
            <section class="card admin-users">
                <div class="card-section-head"><h3>{Html.Encode(t("admin.users.title"))}</h3><span class="rule"></span><span class="count">{users.Count}</span></div>
                {head}
                <div class="admin-user-list">{rows}</div>
            </section>
            """;

        var breadcrumb = Html.Breadcrumb(
            Crumb.Link(t("nav.admin"), "/admin"),
            Crumb.Current(t("admin.users.title")));

        var body = breadcrumb + Html.Raw(section);
        return Layout.Page(t("admin.users.title"), body, ctx);
    }

    public static string UserDetail(User user, IReadOnlyList<Role> allRoles, IReadOnlyList<string> errors, ViewContext ctx)
    {
        var (_, t, csrfToken) = ctx;

        var errorHtml = errors.Count == 0
            ? HtmlString.Empty
            : Html.Div("errors", Html.Ul(errors.Select(Html.Text)));

        HtmlString CheckboxHtml(Role r) =>
            Html.Label(Html.InputCheckbox("role", r.Name, user.RoleNames.Contains(r.Name)) + Html.Raw(" ") + Html.Text(r.Name));

        var roleCheckboxes = HtmlString.Concat(allRoles.Select(CheckboxHtml));
        var formBody = Html.CsrfField(csrfToken) + Html.Fieldset(t("admin.users.roles_label"), roleCheckboxes) + Html.Button(t("admin.users.save"));
        var form = Html.Form($"/admin/users/{user.Id.Value}/roles", formBody);

        var breadcrumb = Html.Breadcrumb(
            Crumb.Link(t("nav.admin"), "/admin"),
            Crumb.Link(t("admin.users.title"), "/admin/users"),
            Crumb.Current(user.Email.Value));

        var body = breadcrumb +
            Html.H1(user.Email.Value) +
            errorHtml +
            form;
        return Layout.Page($"{t("admin.users.detail_title")}: {user.Email.Value}", body, ctx);
    }

    public static string RoleList(IReadOnlyList<Role> roles, ViewContext ctx)
    {
        var (_, t, _) = ctx;

        static HtmlString RoleRow(Role r) =>
            Html.Link($"/admin/roles/{r.Id.Value}", r.Name) +
            Html.Raw($" ({r.Rules.Count})");

        var breadcrumb = Html.Breadcrumb(
            Crumb.Link(t("nav.admin"), "/admin"),
            Crumb.Current(t("admin.roles.title")));

        var body = breadcrumb +
            Html.H1(t("admin.roles.title")) +
            Html.P(Html.Link("/admin/roles/new", t("admin.roles.new"))) +
            Html.P(Html.Link("/admin/users", t("admin.roles.manage_users"))) +
            Html.Ul(roles.Select(RoleRow));
        return Layout.Page(t("admin.roles.title"), body, ctx);
    }

    public static string NewRoleForm(IReadOnlyList<string> errors, ViewContext ctx)
    {
        var (_, t, csrfToken) = ctx;

        var errorHtml = errors.Count == 0
            ? HtmlString.Empty
            : Html.Div("errors", Html.Ul(errors.Select(Html.Text)));

        var formBody = Html.CsrfField(csrfToken) + Html.Label(Html.Text(t("admin.roles.name")) + Html.Input("name")) + Html.Button(t("admin.roles.create"));
        var breadcrumb = Html.Breadcrumb(
            Crumb.Link(t("nav.admin"), "/admin"),
            Crumb.Link(t("admin.roles.title"), "/admin/roles"),
            Crumb.Current(t("common.new")));
        var body = breadcrumb +
            Html.H1(t("admin.roles.new")) +
            errorHtml +
            Html.Form("/admin/roles", formBody);
        return Layout.Page(t("admin.roles.new"), body, ctx);
    }

    public static string RoleDetail(Role role, IReadOnlyList<string> errors, ViewContext ctx)
    {
        var (_, t, csrfToken) = ctx;

        var errorHtml = errors.Count == 0
            ? HtmlString.Empty
            : Html.Div("errors", Html.Ul(errors.Select(Html.Text)));

        var actionOptions = string.Join(
            string.Empty,
            AllActions.Select(a => $"<option value=\"{Html.Encode(a)}\">{Html.Encode(a)}</option>"));

        var resourceOptions = string.Join(
            string.Empty,
            AllResources.Select(r => $"<option value=\"{Html.Encode(r)}\">{Html.Encode(r)}</option>"));

        HtmlString RuleRow(PermissionRule r)
        {
            var ruleFormBody =
                Html.CsrfField(csrfToken) +
                Html.InputHidden("action", r.ActionName) +
                Html.InputHidden("resource", r.ResourceKey) +
                Html.Button(t("admin.roles.remove_rule"));
            return Html.Text(r.ActionName) + Html.Raw(" → ") + Html.Text(r.ResourceKey) + Html.Raw(" ") +
                Html.Form($"/admin/roles/{role.Id.Value}/rules/delete", ruleFormBody, style: "display:inline");
        }

        var ruleRows = role.Rules.Count == 0
            ? Html.P(Html.Text(t("admin.roles.no_rules")))
            : Html.Ul(role.Rules.Select(RuleRow));

        var addFormBody =
            Html.CsrfField(csrfToken) +
            Html.Label(Html.Text(t("admin.roles.action")) + Html.Raw($"""<select name="action">{actionOptions}</select>""")) +
            Html.Label(Html.Text(t("admin.roles.resource")) + Html.Raw($"""<select name="resource">{resourceOptions}</select>""")) +
            Html.Button(t("admin.roles.add_rule"));
        var addForm = Html.Form($"/admin/roles/{role.Id.Value}/rules", addFormBody);
        var deleteForm = Html.Form($"/admin/roles/{role.Id.Value}/delete", Html.CsrfField(csrfToken) + Html.Button(t("admin.roles.delete")));

        var breadcrumb = Html.Breadcrumb(
            Crumb.Link(t("nav.admin"), "/admin"),
            Crumb.Link(t("admin.roles.title"), "/admin/roles"),
            Crumb.Current(role.Name));

        var body = breadcrumb +
            Html.H1(role.Name) +
            Html.P(deleteForm) +
            Html.H2(Html.Text(t("admin.roles.rules"))) +
            ruleRows +
            errorHtml +
            addForm;
        return Layout.Page($"{t("admin.roles.detail_title")}: {role.Name}", body, ctx);
    }

    private static string UserRow(User u, Translate t)
    {
        var hasRoles = u.RoleNames.Count > 0;
        var roles = hasRoles ? string.Join(", ", u.RoleNames) : t("admin.users.no_roles");
        var rolesClass = hasRoles ? "admin-user-cell" : "admin-user-cell muted";

        return $"""
            <div class="admin-user-row">
                <span class="admin-user-cell" data-label="{Html.Encode(t("admin.users.col_user"))}">{Html.Encode(u.DisplayName.Value)}</span>
                <span class="admin-user-cell mono" data-label="{Html.Encode(t("admin.users.col_email"))}">{Html.Encode(u.Email.Value)}</span>
                <span class="{rolesClass}" data-label="{Html.Encode(t("admin.users.col_roles"))}">{Html.Encode(roles)}</span>
                <span class="unit-actions">
                    <a class="icon-btn" href="/admin/users/{u.Id.Value}" title="{Html.Encode(t("common.edit"))}" aria-label="{Html.Encode(t("common.edit"))}">{PencilIcon}</a>
                </span>
            </div>
            """;
    }
}
