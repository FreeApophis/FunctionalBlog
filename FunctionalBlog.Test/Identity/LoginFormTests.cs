namespace FunctionalBlog.Test.Identity;

public sealed class LoginFormTests
{
    [Fact]
    public void Valid_form_is_valid()
    {
        var form = Decode("user@blog.de", "geheim123");

        Assert.True(form.IsValid);
        Assert.Empty(form.Errors);
    }

    [Fact]
    public void Missing_email_adds_error()
    {
        var form = Decode(string.Empty, "geheim123");

        Assert.False(form.IsValid);
        Assert.NotEmpty(form.Errors);
    }

    [Fact]
    public void Missing_password_adds_error()
    {
        var form = Decode("user@blog.de", string.Empty);

        Assert.False(form.IsValid);
        Assert.NotEmpty(form.Errors);
    }

    private static DecodedLoginForm Decode(string email, string password) =>
        LoginForm.Decode(new Request(
            "POST",
            "/login",
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string> { ["email"] = email, ["password"] = password },
            new Dictionary<string, string>()));
}
