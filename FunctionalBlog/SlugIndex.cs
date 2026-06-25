using System.Globalization;

namespace FunctionalBlog;

// Per-request lookup of entity slugs for building canonical view URLs in the rendering layer.
// Built once per request from ISlugRepository (see SlugMiddleware) and threaded into views via
// ViewContext. Falls back to the numeric id when an entity has no registered slug — or when no
// slug repository is configured at all (minimal test envs use SlugIndex.Empty) — so URL
// generation always produces a working path.
public sealed class SlugIndex
{
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<int, string>> _byType;

    public SlugIndex(IReadOnlyDictionary<string, IReadOnlyDictionary<int, string>> byType) => _byType = byType;

    public static SlugIndex Empty { get; } = new(new Dictionary<string, IReadOnlyDictionary<int, string>>());

    // The slug for an entity, or its numeric id as a string when none is registered.
    public string For(string entityType, int id) =>
        _byType.GetValueOrNone(entityType)
            .SelectMany(map => map.GetValueOrNone(id))
            .GetOrElse(id.ToString(CultureInfo.InvariantCulture));

    // The canonical path for an entity, e.g. "/recipes/ruehrkuchen".
    public string Url(string entityType, int id) => $"/{Prefix(entityType)}/{For(entityType, id)}";

    private static string Prefix(string entityType) => entityType switch
    {
        SlugEntityType.Article => "articles",
        SlugEntityType.Recipe => "recipes",
        SlugEntityType.Page => "pages",
        SlugEntityType.Ingredient => "ingredients",
        SlugEntityType.Tag => "tag",
        _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, "Unknown slug entity type"),
    };
}
