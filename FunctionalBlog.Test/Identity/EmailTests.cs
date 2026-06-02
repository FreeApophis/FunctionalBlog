namespace FunctionalBlog.Test.Identity;

public sealed class EmailTests
{
    [Fact]
    public void Parse_returns_email_with_normalized_value_for_valid_address()
    {
        Assert.Equal(Option.Some(new Email("user@example.com")), Email.Parse("User@Example.COM"));
    }

    [Fact]
    public void Parse_trims_surrounding_whitespace()
    {
        Assert.Equal(Option.Some(new Email("test@example.com")), Email.Parse("  test@example.com  "));
    }

    [Fact]
    public void Parse_returns_null_for_missing_at_sign()
    {
        Assert.Equal(Option<Email>.None, Email.Parse("notanemail"));
    }

    [Fact]
    public void Parse_returns_null_for_empty_local_part()
    {
        Assert.Equal(Option<Email>.None, Email.Parse("@example.com"));
    }

    [Fact]
    public void Parse_returns_null_for_missing_dot_in_domain()
    {
        Assert.Equal(Option<Email>.None, Email.Parse("user@example"));
    }

    [Fact]
    public void Parse_returns_null_for_empty_string()
    {
        Assert.Equal(Option<Email>.None, Email.Parse(string.Empty));
    }
}
