namespace FunctionalBlog;

public static class NavViews
{
    public static string Nav(IPrincipal principal)
    {
        var links = Html.Link("/", "Blog");

        if (principal is AuthenticatedUser user)
        {
            var settingsLink = Html.Link("/settings", "Einstellungen");
            var logoutForm =
                """<form method="post" action="/logout" style="display:inline">""" +
                """<button type="submit">Abmelden</button>""" +
                "</form>";
            var adminLink = principal.Can<Manage>(new UserResource())
                ? " · " + Html.Link("/admin/users", "Admin")
                : string.Empty;

            links += $" · {Html.Encode(user.DisplayName.Value)} · {settingsLink} · {logoutForm}{adminLink}";
        }
        else
        {
            links += " · " + Html.Link("/login", "Anmelden") + " · " + Html.Link("/register", "Registrieren");
        }

        return $"<nav>{links}</nav>";
    }
}
