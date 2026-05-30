namespace FunctionalBlog.Identity;

public static class UserSettingsViews
{
    public static string Settings(AuthenticatedUser user, IReadOnlyList<string> errors, IPrincipal principal, Translate t)
    {
        var body = Html.H1(t("settings.title")) +
            Html.P($"{Html.Encode(t("settings.logged_in_as"))}: {Html.Encode(user.DisplayName.Value)} ({Html.Encode(user.Email.Value)})") +
            ChangePasswordSection(errors, t);
        return Layout.Page(t("settings.title"), body, principal, t);
    }

    private static string ChangePasswordSection(IReadOnlyList<string> errors, Translate t)
    {
        var errorHtml = errors.Count == 0
            ? string.Empty
            : Html.Div("errors", Html.Ul(errors.Select(key => t(key))));
        return Html.H2(t("settings.change_password.title")) +
            errorHtml +
            $"""
            <form method="post" action="/settings/password">
                <label>
                    {Html.Encode(t("settings.change_password.current"))}
                    <input type="password" name="current" />
                </label>
                <label>
                    {Html.Encode(t("settings.change_password.new"))}
                    <input type="password" name="password" />
                </label>
                <label>
                    {Html.Encode(t("settings.change_password.confirm"))}
                    <input type="password" name="confirmation" />
                </label>
                <button type="submit">{Html.Encode(t("settings.change_password.submit"))}</button>
            </form>
            """;
    }
}
