namespace FunctionalBlog.Test.Identity;

public sealed class RegisterFormTests
{
    [Fact]
    public void Valid_form_is_valid()
    {
        var form = Decode("user@blog.de", "Maximilian", "geheim123", "geheim123");

        Assert.True(form.IsValid);
        Assert.Empty(form.Errors);
    }

    [Fact]
    public void Missing_email_adds_error()
    {
        var form = Decode(string.Empty, "Maximilian", "geheim123", "geheim123");

        Assert.False(form.IsValid);
        Assert.NotEmpty(form.Errors);
    }

    [Fact]
    public void Invalid_email_format_adds_error()
    {
        var form = Decode("notanemail", "Maximilian", "geheim123", "geheim123");

        Assert.False(form.IsValid);
        Assert.NotEmpty(form.Errors);
    }

    [Fact]
    public void Missing_display_name_adds_error()
    {
        var form = Decode("user@blog.de", string.Empty, "geheim123", "geheim123");

        Assert.False(form.IsValid);
        Assert.NotEmpty(form.Errors);
    }

    [Fact]
    public void Display_name_shorter_than_2_characters_adds_error()
    {
        var form = Decode("user@blog.de", "X", "geheim123", "geheim123");

        Assert.False(form.IsValid);
        Assert.NotEmpty(form.Errors);
    }

    [Fact]
    public void Password_shorter_than_8_characters_adds_error()
    {
        var form = Decode("user@blog.de", "Maximilian", "kurz", "kurz");

        Assert.False(form.IsValid);
        Assert.NotEmpty(form.Errors);
    }

    [Fact]
    public void Mismatched_password_confirmation_adds_error()
    {
        var form = Decode("user@blog.de", "Maximilian", "geheim123", "anders456");

        Assert.False(form.IsValid);
        Assert.NotEmpty(form.Errors);
    }

    private static DecodedRegisterForm Decode(string email, string displayName, string password, string confirmation) =>
        RegisterForm.Decode(new Request(
            HttpMethod.Post,
            "/register",
            new Dictionary<string, string>(),
            new Dictionary<string, string>(),
            new Dictionary<string, string> { ["email"] = email, ["displayName"] = displayName, ["password"] = password, ["confirmation"] = confirmation },
            new Dictionary<string, string>()));
}
