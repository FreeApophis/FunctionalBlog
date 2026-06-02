namespace FunctionalBlog.Identity;

public static class UserSettingsHandlers
{
    public static App Settings => _ => env =>
    {
        var user = (AuthenticatedUser)env.CurrentUser;
        return ValueTask.FromResult(Response.Html(
            UserSettingsViews.Settings(user, [], env.CurrentUser, env.T)));
    };

    public static App ChangePassword => request => async env =>
    {
        var user = (AuthenticatedUser)env.CurrentUser;
        var decoded = ChangePasswordForm.Decode(request);

        if (!decoded.IsValid)
        {
            return Response.Html(
                UserSettingsViews.Settings(user, decoded.Errors, env.CurrentUser, env.T),
                400);
        }

        var stored = (await env.Users.FindById(user.Id)).Match(none: () => default(User), some: u => u);
        if (stored is null || !env.PasswordHasher.Verify(decoded.CurrentPassword, stored.PasswordHash))
        {
            return Response.Html(
                UserSettingsViews.Settings(user, ["auth.error.current_password_wrong"], env.CurrentUser, env.T),
                400);
        }

        await env.Users.Save(stored with { PasswordHash = env.PasswordHasher.Hash(decoded.NewPassword) });
        return Response.Redirect("/settings");
    };
}
