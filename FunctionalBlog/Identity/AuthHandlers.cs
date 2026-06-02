using System.Collections.Immutable;
using System.Security.Cryptography;

namespace FunctionalBlog.Identity;

public static class AuthHandlers
{
    private static readonly string DummyHash = new Pbkdf2PasswordHasher().Hash("timing-dummy");

    public static App NewRegisterForm => _ => env =>
        ValueTask.FromResult(Response.Html(
            AuthViews.RegisterForm([], string.Empty, string.Empty, env.CurrentUser, env.T)));

    public static App Register => request => async env =>
    {
        var decoded = RegisterForm.Decode(request);

        if (!decoded.IsValid)
        {
            return Response.Html(
                AuthViews.RegisterForm(
                    decoded.Errors,
                    request.Form.GetValueOrNone("email").GetOrElse(string.Empty),
                    request.Form.GetValueOrNone("displayName").GetOrElse(string.Empty),
                    env.CurrentUser,
                    env.T),
                400);
        }

        if (decoded.Email is [var regEmail])
        {
            if ((await env.Users.FindByEmail(regEmail)) is not [var existingUser])
            {
                var id = await env.Users.NextId();
                var hash = env.PasswordHasher.Hash(decoded.Password);
                var roleNames = (await env.Roles.FindByName("Benutzer")).Select(role => role.Name).ToEnumerable().ToImmutableList();

                existingUser = User.Create(id, regEmail!, new DisplayName(decoded.DisplayName), hash, roleNames, env.Clock.Now);
                await env.Users.Save(existingUser);
            }

            var session = NewSession(existingUser.Id, env.Clock.Now);
            await env.Sessions.Save(session);
            return RedirectWithSession("/", session, env.Clock.Now);
        }

        return Response.Html(
            AuthViews.RegisterForm(decoded.Errors, request.Form.GetValueOrNone("email").GetOrElse(string.Empty), request.Form.GetValueOrNone("displayName").GetOrElse(string.Empty), env.CurrentUser, env.T),
            400);
    };

    public static App NewLoginForm => _ => env =>
        ValueTask.FromResult(Response.Html(
            AuthViews.LoginForm([], string.Empty, env.CurrentUser, env.T)));

    public static App Login => request => async env =>
    {
        var decoded = LoginForm.Decode(request);

        if (!decoded.IsValid)
        {
            return Response.Html(
                AuthViews.LoginForm(decoded.Errors, decoded.EmailRaw, env.CurrentUser, env.T),
                400);
        }

        var emailOption = Email.ParseOrNone(decoded.EmailRaw);
        var userOption = await emailOption.Match(
            none: () => Task.FromResult(Option<User>.None),
            some: async email => await env.Users.FindByEmail(email));
        var hashToVerify = userOption.Match(none: () => DummyHash, some: u => u.PasswordHash);
        var passwordMatch = env.PasswordHasher.Verify(decoded.Password, hashToVerify);

        if (userOption is not [var user] || !passwordMatch)
        {
            return Response.Html(
                AuthViews.LoginForm(["auth.error.invalid_credentials"], decoded.EmailRaw, env.CurrentUser, env.T),
                401);
        }

        var session = NewSession(user.Id, env.Clock.Now);
        await env.Sessions.Save(session);
        return RedirectWithSession("/", session, env.Clock.Now);
    };

    public static App Logout => request => async env =>
    {
        if (request.Cookies.GetValueOrNone("session") is [var token])
        {
            await env.Sessions.Delete(token);
        }

        return Response.Redirect("/login").WithCookie(CookieHelper.ExpireSessionCookie());
    };

    public static App NewPasswordResetForm => _ => env =>
        ValueTask.FromResult(Response.Html(
            AuthViews.PasswordResetRequestForm(env.CurrentUser, env.T)));

    public static App RequestPasswordReset => request => async env =>
    {
        var decoded = PasswordResetRequestForm.Decode(request);

        var userOption = await Email.ParseOrNone(decoded.EmailRaw)
            .Match(
                none: () => Task.FromResult(Option<User>.None),
                some: async email => await env.Users.FindByEmail(email));
        await userOption.Match(
            none: () => Task.CompletedTask,
            some: async user =>
            {
                var token = GenerateToken();
                var resetToken = new PasswordResetToken(token, user.Id, env.Clock.Now.AddHours(1), Consumed: false);
                await env.PasswordResets.Save(resetToken);
                env.Log.Info($"[Passwort-Reset] reset-token:{token} für {decoded.EmailRaw}");
            });

        return Response.Html(AuthViews.PasswordResetRequested(env.CurrentUser, env.T));
    };

    public static App NewPasswordResetConfirmForm => request => env =>
    {
        var token = request.Query.GetValueOrDefault("token", string.Empty);
        return ValueTask.FromResult(Response.Html(
            AuthViews.PasswordResetConfirmForm([], token, env.CurrentUser, env.T)));
    };

    public static App ConfirmPasswordReset => request => async env =>
    {
        var decoded = PasswordResetConfirmForm.Decode(request);

        if (!decoded.IsValid)
        {
            return Response.Html(
                AuthViews.PasswordResetConfirmForm(decoded.Errors, decoded.Token, env.CurrentUser, env.T),
                400);
        }

        var tokenInvalid = Response.Html(
            AuthViews.PasswordResetConfirmForm(["auth.error.reset_token_invalid"], string.Empty, env.CurrentUser, env.T),
            400);

        var validToken = (await env.PasswordResets.Find(decoded.Token))
            .SelectMany(t => !t.Consumed && t.ExpiresAt > env.Clock.Now
                ? Option.Some(t)
                : Option<PasswordResetToken>.None);

        return await validToken.Match(
            none: () => Task.FromResult(tokenInvalid),
            some: async token =>
                await (await env.Users.FindById(token.UserId)).Match(
                    none: () => Task.FromResult(Response.NotFound()),
                    some: async user =>
                    {
                        await env.Users.Save(user with { PasswordHash = env.PasswordHasher.Hash(decoded.Password) });
                        await env.PasswordResets.Consume(decoded.Token);
                        return Response.Redirect("/login");
                    }));
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
