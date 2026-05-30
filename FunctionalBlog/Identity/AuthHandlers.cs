using System.Security.Cryptography;

namespace FunctionalBlog.Identity;

public static class AuthHandlers
{
    private static readonly string DummyHash = new Pbkdf2PasswordHasher().Hash("timing-dummy");

    public static App NewRegisterForm => _ => env =>
        ValueTask.FromResult(Response.Html(
            AuthViews.RegisterForm([], string.Empty, string.Empty, env.CurrentUser)));

    public static App Register => request => async env =>
    {
        var decoded = RegisterForm.Decode(request);

        if (!decoded.IsValid)
        {
            return Response.Html(
                AuthViews.RegisterForm(
                    decoded.Errors,
                    request.Form.GetValueOrDefault("email", string.Empty),
                    request.Form.GetValueOrDefault("displayName", string.Empty),
                    env.CurrentUser),
                400);
        }

        var existingUser = await env.Users.FindByEmail(decoded.Email!);

        if (existingUser is null)
        {
            var id = await env.Users.NextId();
            var hash = env.PasswordHasher.Hash(decoded.Password);
            var defaultRole = await env.Roles.FindByName("Benutzer");
            var roleNames = defaultRole is not null
                ? (IReadOnlyList<string>)[defaultRole.Name]
                : [];

            existingUser = User.Create(id, decoded.Email!, new DisplayName(decoded.DisplayName), hash, roleNames, env.Clock.Now);
            await env.Users.Save(existingUser);
        }

        var session = NewSession(existingUser.Id, env.Clock.Now);
        await env.Sessions.Save(session);
        return RedirectWithSession("/", session, env.Clock.Now);
    };

    public static App NewLoginForm => _ => env =>
        ValueTask.FromResult(Response.Html(
            AuthViews.LoginForm([], string.Empty, env.CurrentUser)));

    public static App Login => request => async env =>
    {
        var decoded = LoginForm.Decode(request);

        if (!decoded.IsValid)
        {
            return Response.Html(
                AuthViews.LoginForm(decoded.Errors, decoded.EmailRaw, env.CurrentUser),
                400);
        }

        var email = Email.Parse(decoded.EmailRaw);
        var user = email is not null ? await env.Users.FindByEmail(email) : null;
        var hashToVerify = user?.PasswordHash ?? DummyHash;
        var passwordMatch = env.PasswordHasher.Verify(decoded.Password, hashToVerify);

        if (user is null || !passwordMatch)
        {
            var errors = new[] { "E-Mail-Adresse oder Passwort ist falsch." };
            return Response.Html(
                AuthViews.LoginForm(errors, decoded.EmailRaw, env.CurrentUser),
                401);
        }

        var session = NewSession(user.Id, env.Clock.Now);
        await env.Sessions.Save(session);
        return RedirectWithSession("/", session, env.Clock.Now);
    };

    public static App Logout => request => async env =>
    {
        var token = request.Cookies.GetValueOrDefault("session");

        if (token is not null)
        {
            await env.Sessions.Delete(token);
        }

        return Response.Redirect("/login")
            .WithCookie(CookieHelper.ExpireSessionCookie());
    };

    public static App NewPasswordResetForm => _ => env =>
        ValueTask.FromResult(Response.Html(
            AuthViews.PasswordResetRequestForm(env.CurrentUser)));

    public static App RequestPasswordReset => request => async env =>
    {
        var decoded = PasswordResetRequestForm.Decode(request);
        var email = Email.Parse(decoded.EmailRaw);
        var user = email is not null ? await env.Users.FindByEmail(email) : null;

        if (user is not null)
        {
            var token = GenerateToken();
            var resetToken = new PasswordResetToken(token, user.Id, env.Clock.Now.AddHours(1), Consumed: false);
            await env.PasswordResets.Save(resetToken);
            env.Log.Info($"[Passwort-Reset] reset-token:{token} für {decoded.EmailRaw}");
        }

        return Response.Html(AuthViews.PasswordResetRequested(env.CurrentUser));
    };

    public static App NewPasswordResetConfirmForm => request => env =>
    {
        var token = request.Query.GetValueOrDefault("token", string.Empty);
        return ValueTask.FromResult(Response.Html(
            AuthViews.PasswordResetConfirmForm([], token, env.CurrentUser)));
    };

    public static App ConfirmPasswordReset => request => async env =>
    {
        var decoded = PasswordResetConfirmForm.Decode(request);

        if (!decoded.IsValid)
        {
            return Response.Html(
                AuthViews.PasswordResetConfirmForm(decoded.Errors, decoded.Token, env.CurrentUser),
                400);
        }

        var resetToken = await env.PasswordResets.Find(decoded.Token);

        if (resetToken is null || resetToken.Consumed || resetToken.ExpiresAt <= env.Clock.Now)
        {
            var errors = new[] { "Der Reset-Token ist ungültig oder abgelaufen." };
            return Response.Html(
                AuthViews.PasswordResetConfirmForm(errors, string.Empty, env.CurrentUser),
                400);
        }

        var user = await env.Users.FindById(resetToken.UserId);

        if (user is null)
        {
            return Response.NotFound();
        }

        await env.Users.Save(user with { PasswordHash = env.PasswordHasher.Hash(decoded.Password) });
        await env.PasswordResets.Consume(decoded.Token);

        return Response.Redirect("/login");
    };

    private static Session NewSession(UserId userId, DateTimeOffset now) =>
        new(GenerateToken(), userId, now.AddDays(30));

    private static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static Response RedirectWithSession(string location, Session session, DateTimeOffset now) =>
        Response.Redirect(location)
            .WithCookie(CookieHelper.SessionCookie(session.Token, now.AddDays(30)));
}
