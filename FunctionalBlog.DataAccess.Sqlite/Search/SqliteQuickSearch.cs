using System.Data;
using Dapper;

namespace FunctionalBlog.DataAccess.Sqlite;

// Direct LIKE typeahead over the live tables. Each category runs its own capped, name-ordered query
// and the hits are concatenated in a fixed category order: tags, articles, recipes, ingredients.
public sealed class SqliteQuickSearch : IQuickSearch
{
    private readonly IDbConnection _connection;

    public SqliteQuickSearch(IDbConnection connection) => _connection = connection;

    public async ValueTask<IReadOnlyList<QuickSearchHit>> Search(string term)
    {
        var trimmed = term.Trim();
        if (trimmed.Length == 0)
        {
            return [];
        }

        var like = ToLikePattern(trimmed);
        var hits = new List<QuickSearchHit>();

        var tags = await _connection.QueryAsync<TagRow>(
            "SELECT slug AS Slug, name AS Name FROM tags " +
            "WHERE name LIKE @like ESCAPE '\\' ORDER BY name COLLATE NOCASE LIMIT 2",
            new { like });
        hits.AddRange(tags.Select(row => new QuickSearchHit("tag", row.Name, Slug: row.Slug)));

        var articles = await _connection.QueryAsync<EntityRow>(
            "SELECT id AS Id, title AS Title FROM articles " +
            "WHERE title LIKE @like ESCAPE '\\' OR text LIKE @like ESCAPE '\\' " +
            "ORDER BY title COLLATE NOCASE LIMIT 3",
            new { like });
        hits.AddRange(articles.Select(row => new QuickSearchHit("article", row.Title, Id: (int)row.Id)));

        var recipes = await _connection.QueryAsync<EntityRow>(
            "SELECT id AS Id, name AS Title FROM recipes " +
            "WHERE name LIKE @like ESCAPE '\\' ORDER BY name COLLATE NOCASE LIMIT 3",
            new { like });
        hits.AddRange(recipes.Select(row => new QuickSearchHit("recipe", row.Title, Id: (int)row.Id)));

        var ingredients = await _connection.QueryAsync<EntityRow>(
            "SELECT id AS Id, name AS Title FROM ingredients " +
            "WHERE name LIKE @like ESCAPE '\\' ORDER BY name COLLATE NOCASE LIMIT 3",
            new { like });
        hits.AddRange(ingredients.Select(row => new QuickSearchHit("ingredient", row.Title, Id: (int)row.Id)));

        return hits;
    }

    // Wraps the term in wildcards for a substring match, escaping LIKE's own metacharacters so a typed
    // % or _ is matched literally (paired with ESCAPE '\' in every query).
    private static string ToLikePattern(string term)
    {
        var escaped = term
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
        return $"%{escaped}%";
    }

    private sealed record TagRow(string Slug, string Name);

    private sealed record EntityRow(long Id, string Title);
}
