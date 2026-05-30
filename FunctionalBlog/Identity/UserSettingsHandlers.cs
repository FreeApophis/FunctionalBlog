namespace FunctionalBlog.Identity;

public static class UserSettingsHandlers
{
    public static App Settings => _ => env =>
    {
        var user = (AuthenticatedUser)env.CurrentUser;
        return ValueTask.FromResult(Response.Html(
            UserSettingsViews.Settings(user, [], env.CurrentUser)));
    };

    public static App ChangePassword => request => async env =>
    {
        var user = (AuthenticatedUser)env.CurrentUser;
        var decoded = ChangePasswordForm.Decode(request);

        if (!decoded.IsValid)
        {
            return Response.Html(
                UserSettingsViews.Settings(user, decoded.Errors, env.CurrentUser),
                400);
        }

        var stored = await env.Users.FindById(user.Id);

        if (stored is null || !env.PasswordHasher.Verify(decoded.CurrentPassword, stored.PasswordHash))
        {
            var errors = new[] { "Das aktuelle Passwort ist falsch." };
            return Response.Html(
                UserSettingsViews.Settings(user, errors, env.CurrentUser),
                400);
        }

        await env.Users.Save(stored with { PasswordHash = env.PasswordHasher.Hash(decoded.NewPassword) });
        return Response.Redirect("/settings");
    };
}
