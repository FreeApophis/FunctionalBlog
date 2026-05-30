public sealed record Env(
    IArticleRepository Articles,
    IClock Clock,
    ILog Log);
