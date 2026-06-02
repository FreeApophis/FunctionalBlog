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
        var formBody =
            Html.Label(Html.Encode(t("settings.change_password.current")) + Html.InputPassword("current")) +
            Html.Label(Html.Encode(t("settings.change_password.new")) + Html.InputPassword("password")) +
            Html.Label(Html.Encode(t("settings.change_password.confirm")) + Html.InputPassword("confirmation")) +
            Html.Button(t("settings.change_password.submit"));
        return Html.H2(t("settings.change_password.title")) +
            errorHtml +
            Html.Form("/settings/password", formBody);
    }
}
