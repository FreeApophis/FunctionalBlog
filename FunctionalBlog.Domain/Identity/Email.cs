namespace FunctionalBlog.Domain.Identity;

public sealed record Email(string Value)
{
    public static Email? Parse(string raw)
    {
        var normalized = raw.Trim().ToLowerInvariant();
        var at = normalized.IndexOf('@');

        if (at <= 0 || at >= normalized.Length - 2)
        {
            return null;
        }

        var dot = normalized.LastIndexOf('.');

        if (dot <= at + 1 || dot >= normalized.Length - 1)
        {
            return null;
        }

        return new Email(normalized);
    }
}
