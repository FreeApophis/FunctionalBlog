namespace FunctionalBlog.Roles;

public static class AdminViews
{
    private static readonly string[] AllActions = ["View", "Create", "Edit", "Delete", "Manage"];
    private static readonly string[] AllResources = ["article", "user", "role", "rule"];

    public static string UserList(IReadOnlyList<User> users, IPrincipal principal)
    {
        static string UserRow(User u) =>
            Html.Link($"/admin/users/{u.Id.Value}", Html.Encode(u.Email.Value)) +
            " – " + Html.Encode(string.Join(", ", u.RoleNames));

        var body = Html.H1("Benutzer") +
            Html.P(Html.Link("/admin/roles", "Rollen verwalten")) +
            Html.Ul(users.Select(UserRow));
        return Layout.Page("Benutzer", body, principal);
    }

    public static string UserDetail(
        User user,
        IReadOnlyList<Role> allRoles,
        IReadOnlyList<string> errors,
        IPrincipal principal)
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(Html.Encode)));

        static string Checkbox(Role r, User u)
        {
            var checkedAttr = u.RoleNames.Contains(r.Name) ? " checked" : string.Empty;
            return $"<label><input type=\"checkbox\" name=\"role\" value=\"{Html.Encode(r.Name)}\"{checkedAttr} /> {Html.Encode(r.Name)}</label>";
        }

        var roleCheckboxes = string.Join(string.Empty, allRoles.Select(r => Checkbox(r, user)));
        var form = $"""
            <form method="post" action="/admin/users/{user.Id.Value}/roles">
                <fieldset>
                    <legend>Rollen</legend>
                    {roleCheckboxes}
                </fieldset>
                <button type="submit">Speichern</button>
            </form>
            """;
        var body = Html.H1(Html.Encode(user.Email.Value)) +
            Html.P(Html.Link("/admin/users", "← Zurück")) +
            errorHtml +
            form;
        return Layout.Page($"Benutzer: {user.Email.Value}", body, principal);
    }

    public static string RoleList(IReadOnlyList<Role> roles, IPrincipal principal)
    {
        static string RoleRow(Role r) =>
            Html.Link($"/admin/roles/{r.Id.Value}", Html.Encode(r.Name)) +
            $" ({r.Rules.Count} Regeln)";

        var body = Html.H1("Rollen") +
            Html.P(Html.Link("/admin/roles/new", "Neue Rolle erstellen")) +
            Html.P(Html.Link("/admin/users", "Benutzer verwalten")) +
            Html.Ul(roles.Select(RoleRow));
        return Layout.Page("Rollen", body, principal);
    }

    public static string NewRoleForm(IReadOnlyList<string> errors, IPrincipal principal)
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(Html.Encode)));
        var body = Html.H1("Neue Rolle") +
            Html.P(Html.Link("/admin/roles", "← Zurück")) +
            errorHtml +
            """
            <form method="post" action="/admin/roles">
                <label>
                    Name
                    <input name="name" />
                </label>
                <button type="submit">Erstellen</button>
            </form>
            """;
        return Layout.Page("Neue Rolle", body, principal);
    }

    public static string RoleDetail(Role role, IReadOnlyList<string> errors, IPrincipal principal)
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

        static string RuleRow(PermissionRule r, int roleId) =>
            $"{Html.Encode(r.ActionName)} auf {Html.Encode(r.ResourceKey)} " +
            $"<form method=\"post\" action=\"/admin/roles/{roleId}/rules/delete\" style=\"display:inline\">" +
            $"<input type=\"hidden\" name=\"action\" value=\"{Html.Encode(r.ActionName)}\" />" +
            $"<input type=\"hidden\" name=\"resource\" value=\"{Html.Encode(r.ResourceKey)}\" />" +
            "<button type=\"submit\">Entfernen</button></form>";

        var ruleRows = role.Rules.Count == 0
            ? Html.P("Keine Regeln vorhanden.")
            : Html.Ul(role.Rules.Select(r => RuleRow(r, role.Id.Value)));

        var addForm = $"""
            <form method="post" action="/admin/roles/{role.Id.Value}/rules">
                <label>
                    Aktion
                    <select name="action">{actionOptions}</select>
                </label>
                <label>
                    Ressource
                    <select name="resource">{resourceOptions}</select>
                </label>
                <button type="submit">Regel hinzufügen</button>
            </form>
            """;

        var deleteForm =
            $"<form method=\"post\" action=\"/admin/roles/{role.Id.Value}/delete\">" +
            "<button type=\"submit\">Rolle löschen</button></form>";

        var body = Html.H1(Html.Encode(role.Name)) +
            Html.P(Html.Link("/admin/roles", "← Zurück")) +
            Html.P(deleteForm) +
            Html.H2("Regeln") +
            ruleRows +
            errorHtml +
            addForm;
        return Layout.Page($"Rolle: {role.Name}", body, principal);
    }
}
