namespace FunctionalBlog.Identity;

public static class AuthViews
{
    public static string RegisterForm(IReadOnlyList<string> errors, string email, string displayName, IPrincipal principal, Translate t)
    {
        var fields = $"""
            <label>
                {Html.Encode(t("auth.register.display_name"))}
                <input type="text" name="displayName" value="{Html.Encode(displayName)}" />
            </label>
            <label>
                {Html.Encode(t("auth.register.email"))}
                <input type="email" name="email" value="{Html.Encode(email)}" />
            </label>
            <label>
                {Html.Encode(t("auth.register.password"))}
                <input type="password" name="password" />
            </label>
            <label>
                {Html.Encode(t("auth.register.confirm_password"))}
                <input type="password" name="confirmation" />
            </label>
            <button type="submit">{Html.Encode(t("auth.register.submit"))}</button>
            <p>{Html.Link("/login", t("auth.register.already_registered"))}</p>
            """;
        var body = FormBody(t("auth.register.title"), "/register", errors, fields, t);
        return Layout.Page(t("auth.register.title"), body, principal, t);
    }

    public static string LoginForm(IReadOnlyList<string> errors, string email, IPrincipal principal, Translate t)
    {
        var fields = $"""
            <label>
                {Html.Encode(t("auth.register.email"))}
                <input type="email" name="email" value="{Html.Encode(email)}" />
            </label>
            <label>
                {Html.Encode(t("auth.register.password"))}
                <input type="password" name="password" />
            </label>
            <button type="submit">{Html.Encode(t("auth.login.title"))}</button>
            <p>{Html.Link("/register", t("auth.login.no_account"))}</p>
            <p>{Html.Link("/password-reset", t("auth.login.forgot_password"))}</p>
            """;
        var body = FormBody(t("auth.login.title"), "/login", errors, fields, t);
        return Layout.Page(t("auth.login.title"), body, principal, t);
    }

    public static string PasswordResetRequestForm(IPrincipal principal, Translate t)
    {
        var fields = $"""
            <label>
                {Html.Encode(t("auth.reset.email"))}
                <input type="email" name="email" />
            </label>
            <button type="submit">{Html.Encode(t("auth.reset.submit"))}</button>
            <p>{Html.Link("/login", t("auth.reset.back"))}</p>
            """;
        var body = FormBody(t("auth.reset.title"), "/password-reset", [], fields, t);
        return Layout.Page(t("auth.reset.title"), body, principal, t);
    }

    public static string PasswordResetRequested(IPrincipal principal, Translate t)
    {
        var body = Html.P(t("auth.reset.sent")) +
            Html.P(Html.Link("/login", t("auth.reset.back")));
        return Layout.Page(t("auth.reset.title"), body, principal, t);
    }

    public static string PasswordResetConfirmForm(IReadOnlyList<string> errors, string token, IPrincipal principal, Translate t)
    {
        var fields = $"""
            <input type="hidden" name="token" value="{Html.Encode(token)}" />
            <label>
                {Html.Encode(t("auth.reset_confirm.new_password"))}
                <input type="password" name="password" />
            </label>
            <label>
                {Html.Encode(t("auth.reset_confirm.confirm_password"))}
                <input type="password" name="confirmation" />
            </label>
            <button type="submit">{Html.Encode(t("auth.reset_confirm.submit"))}</button>
            """;
        var body = FormBody(t("auth.reset_confirm.title"), "/password-reset/confirm", errors, fields, t);
        return Layout.Page(t("auth.reset_confirm.page_title"), body, principal, t);
    }

    private static string FormBody(string title, string action, IReadOnlyList<string> errors, string fields, Translate t)
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(key => t(key))));
        return Html.H1(title) +
            errorHtml +
            $"""
            <form method="post" action="{Html.Encode(action)}">
                {fields}
            </form>
            """;
    }
}
