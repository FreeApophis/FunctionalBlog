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

        string Checkbox(Role r, User u)
        {
            var checkedAttr = u.RoleNames.Contains(r.Name) ? " checked" : string.Empty;
            return $"<label><input type=\"checkbox\" name=\"role\" value=\"{Html.Encode(r.Name)}\"{checkedAttr} /> {Html.Encode(r.Name)}</label>";
        }

        var roleCheckboxes = string.Join(string.Empty, allRoles.Select(r => Checkbox(r, user)));
        var form = $"""
            <form method="post" action="/admin/users/{user.Id.Value}/roles">
                <fieldset>
                    <legend>{Html.Encode(t("admin.users.roles_label"))}</legend>
                    {roleCheckboxes}
                </fieldset>
                <button type="submit">{Html.Encode(t("admin.users.save"))}</button>
            </form>
            """;
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
        var body = Html.H1(t("admin.roles.new")) +
            Html.P(Html.Link("/admin/roles", t("common.back"))) +
            errorHtml +
            $"""
            <form method="post" action="/admin/roles">
                <label>
                    {Html.Encode(t("admin.roles.name"))}
                    <input name="name" />
                </label>
                <button type="submit">{Html.Encode(t("admin.roles.create"))}</button>
            </form>
            """;
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

        string RuleRow(PermissionRule r) =>
            $"{Html.Encode(r.ActionName)} → {Html.Encode(r.ResourceKey)} " +
            $"<form method=\"post\" action=\"/admin/roles/{role.Id.Value}/rules/delete\" style=\"display:inline\">" +
            $"<input type=\"hidden\" name=\"action\" value=\"{Html.Encode(r.ActionName)}\" />" +
            $"<input type=\"hidden\" name=\"resource\" value=\"{Html.Encode(r.ResourceKey)}\" />" +
            $"<button type=\"submit\">{Html.Encode(t("admin.roles.remove_rule"))}</button></form>";

        var ruleRows = role.Rules.Count == 0
            ? Html.P(t("admin.roles.no_rules"))
            : Html.Ul(role.Rules.Select(RuleRow));

        var addForm = $"""
            <form method="post" action="/admin/roles/{role.Id.Value}/rules">
                <label>
                    {Html.Encode(t("admin.roles.action"))}
                    <select name="action">{actionOptions}</select>
                </label>
                <label>
                    {Html.Encode(t("admin.roles.resource"))}
                    <select name="resource">{resourceOptions}</select>
                </label>
                <button type="submit">{Html.Encode(t("admin.roles.add_rule"))}</button>
            </form>
            """;

        var deleteForm =
            $"<form method=\"post\" action=\"/admin/roles/{role.Id.Value}/delete\">" +
            $"<button type=\"submit\">{Html.Encode(t("admin.roles.delete"))}</button></form>";

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
