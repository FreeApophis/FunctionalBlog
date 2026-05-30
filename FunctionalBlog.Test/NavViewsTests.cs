namespace FunctionalBlog.Test;

public sealed class NavViewsTests
{
    [Fact]
    public void Nav_for_guest_contains_anmelden_link()
    {
        var nav = NavViews.Nav(Guest.Instance);

        Assert.Contains("/login", nav);
        Assert.Contains("Anmelden", nav);
    }

    [Fact]
    public void Nav_for_guest_contains_registrieren_link()
    {
        var nav = NavViews.Nav(Guest.Instance);

        Assert.Contains("/register", nav);
        Assert.Contains("Registrieren", nav);
    }

    [Fact]
    public void Nav_for_authenticated_user_contains_abmelden_form()
    {
        var user = BuildAuthUser([]);

        var nav = NavViews.Nav(user);

        Assert.Contains("/logout", nav);
        Assert.Contains("Abmelden", nav);
    }

    [Fact]
    public void Nav_for_authenticated_user_shows_display_name()
    {
        var user = BuildAuthUser([]);

        var nav = NavViews.Nav(user);

        Assert.Contains("Testbenutzer", nav);
    }

    [Fact]
    public void Nav_for_admin_contains_admin_link()
    {
        var rule = new PermissionRule("Manage", "user");
        var role = new Role(new RoleId(1), "Admin", [rule]);
        var user = BuildAuthUser([role]);

        var nav = NavViews.Nav(user);

        Assert.Contains("/admin/users", nav);
        Assert.Contains("Admin", nav);
    }

    [Fact]
    public void Nav_for_non_admin_does_not_contain_admin_link()
    {
        var user = BuildAuthUser([]);

        var nav = NavViews.Nav(user);

        Assert.DoesNotContain("/admin/users", nav);
    }

    private static AuthenticatedUser BuildAuthUser(IReadOnlyList<Role> roles)
    {
        var user = User.Create(
            new UserId(1),
            new Email("test@blog.de"),
            new DisplayName("Testbenutzer"),
            "hash",
            [],
            DateTimeOffset.UtcNow);
        return new AuthenticatedUser(user, roles);
    }
}
