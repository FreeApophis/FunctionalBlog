namespace FunctionalBlog.Roles;

public static class AdminViews
{
    private static readonly string[] AllActions = ["View", "Create", "Edit", "Delete", "Manage"];
    private static readonly string[] AllResources = ["article", "user", "role", "rule"];

    public static string UserList(IReadOnlyList<User> users, ViewContext ctx)
    {
        var (_, t, _) = ctx;

        static HtmlString UserRow(User u) =>
            Html.Link($"/admin/users/{u.Id.Value}", $"{u.DisplayName.Value} ({u.Email.Value})") +
            Html.Raw(" – ") + Html.Text(string.Join(", ", u.RoleNames));

        var body = Html.H1(t("admin.users.title")) +
            Html.P(Html.Link("/admin/roles", t("admin.users.manage_roles"))) +
            Html.P(Html.Link("/admin/ingredients", t("ingredient.list_title"))) +
            Html.P(Html.Link("/admin/translations", t("translations.title"))) +
            Html.Ul(users.Select(UserRow));
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

        var body = Html.H1(user.Email.Value) +
            Html.P(Html.Link("/admin/users", t("common.back"))) +
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

        var body = Html.H1(t("admin.roles.title")) +
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
        var body = Html.H1(t("admin.roles.new")) +
            Html.P(Html.Link("/admin/roles", t("common.back"))) +
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

        var body = Html.H1(role.Name) +
            Html.P(Html.Link("/admin/roles", t("common.back"))) +
            Html.P(deleteForm) +
            Html.H2(Html.Text(t("admin.roles.rules"))) +
            ruleRows +
            errorHtml +
            addForm;
        return Layout.Page($"{t("admin.roles.detail_title")}: {role.Name}", body, ctx);
    }
}
