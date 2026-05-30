namespace FunctionalBlog.Test;

public sealed class NavViewsTests
{
    [Fact]
    public void Nav_for_guest_contains_login_link()
    {
        var nav = NavViews.Nav(Guest.Instance, NoOp);

        Assert.Contains("/login", nav);
        Assert.Contains("nav.login", nav);
    }

    [Fact]
    public void Nav_for_guest_contains_register_link()
    {
        var nav = NavViews.Nav(Guest.Instance, NoOp);

        Assert.Contains("/register", nav);
        Assert.Contains("nav.register", nav);
    }

    [Fact]
    public void Nav_for_authenticated_user_contains_logout_form()
    {
        var user = BuildAuthUser([]);

        var nav = NavViews.Nav(user, NoOp);

        Assert.Contains("/logout", nav);
        Assert.Contains("nav.logout", nav);
    }

    [Fact]
    public void Nav_for_authenticated_user_shows_display_name()
    {
        var user = BuildAuthUser([]);

        var nav = NavViews.Nav(user, NoOp);

        Assert.Contains("Testbenutzer", nav);
    }

    [Fact]
    public void Nav_for_admin_contains_admin_link()
    {
        var rule = new PermissionRule("Manage", "user");
        var role = new Role(new RoleId(1), "Admin", [rule]);
        var user = BuildAuthUser([role]);

        var nav = NavViews.Nav(user, NoOp);

        Assert.Contains("/admin/users", nav);
        Assert.Contains("nav.admin", nav);
    }

    [Fact]
    public void Nav_for_non_admin_does_not_contain_admin_link()
    {
        var user = BuildAuthUser([]);

        var nav = NavViews.Nav(user, NoOp);

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

    private static readonly Translate NoOp = key => key;
}
