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

        return await ChangePasswordForm.Decode(request).Match(
            failure: f => Task.FromResult(Response.Html(
                UserSettingsViews.Settings(user, f.Error, env.CurrentUser, env.T), 400)),
            success: async s =>
            {
                if ((await env.Users.FindById(user.Id)) is not [var stored] ||
                    !env.PasswordHasher.Verify(s.Value.CurrentPassword, stored.PasswordHash))
                {
                    return Response.Html(
                        UserSettingsViews.Settings(user, ["auth.error.current_password_wrong"], env.CurrentUser, env.T),
                        400);
                }

                await env.Users.Save(stored with { PasswordHash = env.PasswordHasher.Hash(s.Value.NewPassword) });
                return Response.Redirect("/settings");
            });
    };
}
