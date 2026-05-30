namespace FunctionalBlog.Test.Identity;

public sealed class EmailTests
{
    [Fact]
    public void Parse_returns_email_with_normalized_value_for_valid_address()
    {
        var email = Email.Parse("User@Example.COM");

        Assert.NotNull(email);
        Assert.Equal("user@example.com", email.Value);
    }

    [Fact]
    public void Parse_trims_surrounding_whitespace()
    {
        var email = Email.Parse("  test@example.com  ");

        Assert.NotNull(email);
        Assert.Equal("test@example.com", email.Value);
    }

    [Fact]
    public void Parse_returns_null_for_missing_at_sign()
    {
        Assert.Null(Email.Parse("notanemail"));
    }

    [Fact]
    public void Parse_returns_null_for_empty_local_part()
    {
        Assert.Null(Email.Parse("@example.com"));
    }

    [Fact]
    public void Parse_returns_null_for_missing_dot_in_domain()
    {
        Assert.Null(Email.Parse("user@example"));
    }

    [Fact]
    public void Parse_returns_null_for_empty_string()
    {
        Assert.Null(Email.Parse(string.Empty));
    }
}
