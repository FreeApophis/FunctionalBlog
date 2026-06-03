namespace FunctionalBlog.Identity;

public static class UserSettingsViews
{
    public static string Settings(AuthenticatedUser user, IReadOnlyList<string> errors, ViewContext ctx)
    {
        var (_, t, _) = ctx;
        var body = Html.H1(t("settings.title")) +
            Html.P(Html.Text($"{t("settings.logged_in_as")}: {user.DisplayName.Value} ({user.Email.Value})")) +
            ChangePasswordSection(errors, ctx);
        return Layout.Page(t("settings.title"), body, ctx);
    }

    private static HtmlString ChangePasswordSection(IReadOnlyList<string> errors, ViewContext ctx)
    {
        var (_, t, csrfToken) = ctx;
        var errorHtml = errors.Count == 0
            ? HtmlString.Empty
            : Html.Div("errors", Html.Ul(errors.Select(key => Html.Text(t(key)))));
        var formBody =
            Html.CsrfField(csrfToken) +
            Html.Label(Html.Text(t("settings.change_password.current")) + Html.InputPassword("current")) +
            Html.Label(Html.Text(t("settings.change_password.new")) + Html.InputPassword("password")) +
            Html.Label(Html.Text(t("settings.change_password.confirm")) + Html.InputPassword("confirmation")) +
            Html.Button(t("settings.change_password.submit"));
        return Html.H2(Html.Text(t("settings.change_password.title"))) +
            errorHtml +
            Html.Form("/settings/password", formBody);
    }
}
