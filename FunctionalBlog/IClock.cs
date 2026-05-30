namespace FunctionalBlog;

public interface IClock
{
    DateTimeOffset Now { get; }
}
