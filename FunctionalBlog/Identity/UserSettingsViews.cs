namespace FunctionalBlog.Identity;

public static class UserSettingsViews
{
    public static string Settings(AuthenticatedUser user, IReadOnlyList<string> errors, IPrincipal principal)
    {
        var body = Html.H1("Einstellungen") +
            Html.P($"Angemeldet als: {Html.Encode(user.Email.Value)}") +
            ChangePasswordSection(errors);
        return Layout.Page("Einstellungen", body, principal);
    }

    private static string ChangePasswordSection(IReadOnlyList<string> errors)
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(Html.Encode)));
        return Html.H2("Passwort ändern") +
            errorHtml +
            """
            <form method="post" action="/settings/password">
                <label>
                    Aktuelles Passwort
                    <input type="password" name="current" />
                </label>
                <label>
                    Neues Passwort
                    <input type="password" name="password" />
                </label>
                <label>
                    Neues Passwort bestätigen
                    <input type="password" name="confirmation" />
                </label>
                <button type="submit">Passwort ändern</button>
            </form>
            """;
    }
}
