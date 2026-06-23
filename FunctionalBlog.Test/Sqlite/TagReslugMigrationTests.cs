using System.Data;
using Dapper;

namespace FunctionalBlog.Test.Sqlite;

// Verifies migration 0013 repairs databases that were migrated before the slug fix, where
// recipes kept an umlaut slug ("süss") while 0012 created a separate canonical "suess" tag.
public sealed class TagReslugMigrationTests : IDisposable
{
    private readonly SqliteTestBase _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Merges_umlaut_slug_recipe_tag_with_the_ascii_article_tag()
    {
        var conn = _db.Connection;

        // Recreate the buggy production state from the freshly-migrated (correct) one: split the
        // single "suess" tag into a recipe-only "süss" tag and an article-only "suess" tag.
        var suessId = await conn.ExecuteScalarAsync<long>("SELECT id FROM tags WHERE slug = 'suess'");
        var articleTaggable = await conn.ExecuteScalarAsync<long>(
            "SELECT taggable_id FROM articles WHERE title = 'Macarons selbst backen'");

        await conn.ExecuteAsync("UPDATE tags SET slug = 'süss' WHERE id = @id", new { id = suessId });
        await conn.ExecuteAsync("INSERT INTO tags (slug, name) VALUES ('suess', 'süss')");
        var newSuessId = await conn.ExecuteScalarAsync<long>("SELECT last_insert_rowid()");
        await conn.ExecuteAsync(
            "DELETE FROM taggings WHERE taggable_id = @tg AND tag_id = @old",
            new { tg = articleTaggable, old = suessId });
        await conn.ExecuteAsync(
            "INSERT INTO taggings (taggable_id, tag_id) VALUES (@tg, @new)",
            new { tg = articleTaggable, @new = newSuessId });

        // Bug reproduced: /tag/suess sees the article but none of the recipes.
        Assert.Empty(await new SqliteRecipeRepository(conn).FindByTag("suess"));

        await RunEmbeddedScript(conn, "0013_reslug_tags.sql");

        // After the fix: one "suess" tag carrying both the recipes (incl. Apfelrosen) and the article.
        var recipes = await new SqliteRecipeRepository(conn).FindByTag("suess");
        var articles = await new SqliteArticleRepository(conn).FindByTag("suess");

        Assert.Contains(recipes, r => r.Name.Value == "Apfelrosen");
        Assert.Contains(articles, a => a.Title.Value == "Macarons selbst backen");
        Assert.Equal(1, await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM tags WHERE slug = 'suess'"));
        Assert.Equal(0, await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM tags WHERE slug = 'süss'"));
    }

    private static async Task RunEmbeddedScript(IDbConnection conn, string contains)
    {
        var assembly = typeof(DatabaseMigrator).Assembly;
        var resource = assembly.GetManifestResourceNames().Single(n => n.Contains(contains));
        await using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        await conn.ExecuteAsync(await reader.ReadToEndAsync());
    }
}
