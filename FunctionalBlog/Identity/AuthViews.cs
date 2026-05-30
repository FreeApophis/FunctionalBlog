namespace FunctionalBlog.Identity;

public static class AuthViews
{
    public static string RegisterForm(IReadOnlyList<string> errors, string email, IPrincipal principal)
    {
        var fields = $"""
            <label>
                E-Mail
                <input type="email" name="email" value="{Html.Encode(email)}" />
            </label>
            <label>
                Passwort
                <input type="password" name="password" />
            </label>
            <label>
                Passwort bestätigen
                <input type="password" name="confirmation" />
            </label>
            <button type="submit">Konto erstellen</button>
            <p>{Html.Link("/login", "Bereits registriert? Anmelden")}</p>
            """;
        var body = FormBody("Registrieren", "/register", errors, fields);
        return Layout.Page("Registrieren", body, principal);
    }

    public static string LoginForm(IReadOnlyList<string> errors, string email, IPrincipal principal)
    {
        var fields = $"""
            <label>
                E-Mail
                <input type="email" name="email" value="{Html.Encode(email)}" />
            </label>
            <label>
                Passwort
                <input type="password" name="password" />
            </label>
            <button type="submit">Anmelden</button>
            <p>{Html.Link("/register", "Noch kein Konto? Registrieren")}</p>
            <p>{Html.Link("/password-reset", "Passwort vergessen?")}</p>
            """;
        var body = FormBody("Anmelden", "/login", errors, fields);
        return Layout.Page("Anmelden", body, principal);
    }

    public static string PasswordResetRequestForm(IPrincipal principal)
    {
        var fields = $"""
            <label>
                E-Mail
                <input type="email" name="email" />
            </label>
            <button type="submit">Link anfordern</button>
            <p>{Html.Link("/login", "Zurück zur Anmeldung")}</p>
            """;
        var body = FormBody("Passwort zurücksetzen", "/password-reset", [], fields);
        return Layout.Page("Passwort zurücksetzen", body, principal);
    }

    public static string PasswordResetRequested(IPrincipal principal)
    {
        var body = Html.P("Wenn ein Konto mit dieser E-Mail-Adresse existiert, wurde ein Link gesendet.") +
            Html.P(Html.Link("/login", "Zurück zur Anmeldung"));
        return Layout.Page("Passwort zurücksetzen", body, principal);
    }

    public static string PasswordResetConfirmForm(IReadOnlyList<string> errors, string token, IPrincipal principal)
    {
        var fields = $"""
            <input type="hidden" name="token" value="{Html.Encode(token)}" />
            <label>
                Neues Passwort
                <input type="password" name="password" />
            </label>
            <label>
                Passwort bestätigen
                <input type="password" name="confirmation" />
            </label>
            <button type="submit">Passwort ändern</button>
            """;
        var body = FormBody("Neues Passwort festlegen", "/password-reset/confirm", errors, fields);
        return Layout.Page("Neues Passwort", body, principal);
    }

    private static string FormBody(string title, string action, IReadOnlyList<string> errors, string fields)
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(Html.Encode)));
        return Html.H1(title) +
            errorHtml +
            $"""
            <form method="post" action="{Html.Encode(action)}">
                {fields}
            </form>
            """;
    }
}
