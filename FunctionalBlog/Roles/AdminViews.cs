namespace FunctionalBlog.Roles;

public static class AdminViews
{
    private static readonly string[] AllActions = ["View", "Create", "Edit", "Delete", "Manage"];
    private static readonly string[] AllResources = ["article", "user", "role", "rule"];

    public static string UserList(IReadOnlyList<User> users, IPrincipal principal, Translate t)
    {
        static string UserRow(User u) =>
            Html.Link($"/admin/users/{u.Id.Value}", $"{Html.Encode(u.DisplayName.Value)} ({Html.Encode(u.Email.Value)})") +
            " – " + Html.Encode(string.Join(", ", u.RoleNames));

        var body = Html.H1(t("admin.users.title")) +
            Html.P(Html.Link("/admin/roles", t("admin.users.manage_roles"))) +
            Html.P(Html.Link("/admin/ingredients", t("ingredient.list_title"))) +
            Html.P(Html.Link("/admin/translations", t("translations.title"))) +
            Html.Ul(users.Select(UserRow));
        return Layout.Page(t("admin.users.title"), body, principal, t);
    }

    public static string UserDetail(User user, IReadOnlyList<Role> allRoles, IReadOnlyList<string> errors, IPrincipal principal, Translate t)
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(Html.Encode)));

        string CheckboxHtml(Role r) =>
            Html.Label(Html.InputCheckbox("role", r.Name, user.RoleNames.Contains(r.Name)) + " " + Html.Encode(r.Name));

        var roleCheckboxes = string.Concat(allRoles.Select(CheckboxHtml));
        var formBody = Html.Fieldset(t("admin.users.roles_label"), roleCheckboxes) + Html.Button(t("admin.users.save"));
        var form = Html.Form($"/admin/users/{user.Id.Value}/roles", formBody);

        var body = Html.H1(Html.Encode(user.Email.Value)) +
            Html.P(Html.Link("/admin/users", t("common.back"))) +
            errorHtml +
            form;
        return Layout.Page($"Benutzer: {user.Email.Value}", body, principal, t);
    }

    public static string RoleList(IReadOnlyList<Role> roles, IPrincipal principal, Translate t)
    {
        static string RoleRow(Role r) =>
            Html.Link($"/admin/roles/{r.Id.Value}", Html.Encode(r.Name)) +
            $" ({r.Rules.Count})";

        var body = Html.H1(t("admin.roles.title")) +
            Html.P(Html.Link("/admin/roles/new", t("admin.roles.new"))) +
            Html.P(Html.Link("/admin/users", t("admin.roles.manage_users"))) +
            Html.Ul(roles.Select(RoleRow));
        return Layout.Page(t("admin.roles.title"), body, principal, t);
    }

    public static string NewRoleForm(IReadOnlyList<string> errors, IPrincipal principal, Translate t)
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(Html.Encode)));

        var formBody = Html.Label(Html.Encode(t("admin.roles.name")) + Html.Input("name")) + Html.Button(t("admin.roles.create"));
        var body = Html.H1(t("admin.roles.new")) +
            Html.P(Html.Link("/admin/roles", t("common.back"))) +
            errorHtml +
            Html.Form("/admin/roles", formBody);
        return Layout.Page(t("admin.roles.new"), body, principal, t);
    }

    public static string RoleDetail(Role role, IReadOnlyList<string> errors, IPrincipal principal, Translate t)
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(Html.Encode)));

        var actionOptions = string.Join(
            string.Empty,
            AllActions.Select(a => $"<option value=\"{Html.Encode(a)}\">{Html.Encode(a)}</option>"));

        var resourceOptions = string.Join(
            string.Empty,
            AllResources.Select(r => $"<option value=\"{Html.Encode(r)}\">{Html.Encode(r)}</option>"));

        string RuleRow(PermissionRule r)
        {
            var ruleFormBody =
                Html.InputHidden("action", r.ActionName) +
                Html.InputHidden("resource", r.ResourceKey) +
                Html.Button(t("admin.roles.remove_rule"));
            return Html.Encode(r.ActionName) + " → " + Html.Encode(r.ResourceKey) + " " +
                Html.Form($"/admin/roles/{role.Id.Value}/rules/delete", ruleFormBody, style: "display:inline");
        }

        var ruleRows = role.Rules.Count == 0
            ? Html.P(t("admin.roles.no_rules"))
            : Html.Ul(role.Rules.Select(RuleRow));

        var addFormBody =
            Html.Label(Html.Encode(t("admin.roles.action")) + $"""<select name="action">{actionOptions}</select>""") +
            Html.Label(Html.Encode(t("admin.roles.resource")) + $"""<select name="resource">{resourceOptions}</select>""") +
            Html.Button(t("admin.roles.add_rule"));
        var addForm = Html.Form($"/admin/roles/{role.Id.Value}/rules", addFormBody);
        var deleteForm = Html.Form($"/admin/roles/{role.Id.Value}/delete", Html.Button(t("admin.roles.delete")));

        var body = Html.H1(Html.Encode(role.Name)) +
            Html.P(Html.Link("/admin/roles", t("common.back"))) +
            Html.P(deleteForm) +
            Html.H2(t("admin.roles.rules")) +
            ruleRows +
            errorHtml +
            addForm;
        return Layout.Page($"Rolle: {role.Name}", body, principal, t);
    }
}
