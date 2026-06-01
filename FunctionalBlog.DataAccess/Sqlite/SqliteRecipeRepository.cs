using System.Data;
using Dapper;

namespace FunctionalBlog.DataAccess.Sqlite;

public sealed class SqliteRecipeRepository : IRecipeRepository
{
    private readonly IDbConnection _connection;
    private int _nextId = -1;

    public SqliteRecipeRepository(IDbConnection connection) => _connection = connection;

    public async ValueTask<IReadOnlyList<Recipe>> All()
    {
        var rows = (await _connection.QueryAsync<RecipeRow>(
            "SELECT id AS Id, name AS Name, description AS Description, author_id AS AuthorId, difficulty AS Difficulty, portions AS Portions FROM recipes")).ToList();

        if (rows.Count == 0)
        {
            return [];
        }

        var ids = rows.Select(r => r.Id).ToList();

        var steps = (await _connection.QueryAsync<StepRow>(
            "SELECT recipe_id AS RecipeId, sort_order AS SortOrder, text AS Text FROM recipe_steps WHERE recipe_id IN @ids ORDER BY sort_order",
            new { ids })).ToLookup(s => s.RecipeId);

        var tags = (await _connection.QueryAsync<TagRow>(
            "SELECT recipe_id AS RecipeId, tag AS Tag FROM recipe_tags WHERE recipe_id IN @ids",
            new { ids })).ToLookup(t => t.RecipeId);

        var ingredients = (await _connection.QueryAsync<IngredientRow>(
            "SELECT recipe_id AS RecipeId, sort_order AS SortOrder, ingredient_id AS IngredientId, amount AS Amount, unit_abbreviation AS UnitAbbreviation FROM recipe_ingredients WHERE recipe_id IN @ids ORDER BY sort_order",
            new { ids })).ToLookup(i => i.RecipeId);

        var images = (await _connection.QueryAsync<ImageRow>(
            "SELECT recipe_id AS RecipeId, url AS Url FROM recipe_images WHERE recipe_id IN @ids ORDER BY sort_order",
            new { ids })).ToLookup(i => i.RecipeId);

        var hints = (await _connection.QueryAsync<HintRow>(
            "SELECT recipe_id AS RecipeId, text AS Text FROM recipe_hints WHERE recipe_id IN @ids ORDER BY sort_order",
            new { ids })).ToLookup(h => h.RecipeId);

        return rows.Select(r => BuildRecipe(
            r,
            steps[r.Id].ToList(),
            tags[r.Id].ToList(),
            ingredients[r.Id].ToList(),
            images[r.Id].ToList(),
            hints[r.Id].ToList())).ToList();
    }

    public async ValueTask<Recipe?> Find(RecipeId id)
    {
        var row = await _connection.QuerySingleOrDefaultAsync<RecipeRow>(
            "SELECT id AS Id, name AS Name, description AS Description, author_id AS AuthorId, difficulty AS Difficulty, portions AS Portions FROM recipes WHERE id = @id",
            new { id = id.Value });

        if (row is null)
        {
            return null;
        }

        var steps = (await _connection.QueryAsync<StepRow>(
            "SELECT recipe_id AS RecipeId, sort_order AS SortOrder, text AS Text FROM recipe_steps WHERE recipe_id = @id ORDER BY sort_order",
            new { id = id.Value })).ToList();

        var tags = (await _connection.QueryAsync<TagRow>(
            "SELECT recipe_id AS RecipeId, tag AS Tag FROM recipe_tags WHERE recipe_id = @id",
            new { id = id.Value })).ToList();

        var ingredients = (await _connection.QueryAsync<IngredientRow>(
            "SELECT recipe_id AS RecipeId, sort_order AS SortOrder, ingredient_id AS IngredientId, amount AS Amount, unit_abbreviation AS UnitAbbreviation FROM recipe_ingredients WHERE recipe_id = @id ORDER BY sort_order",
            new { id = id.Value })).ToList();

        var images = (await _connection.QueryAsync<ImageRow>(
            "SELECT recipe_id AS RecipeId, url AS Url FROM recipe_images WHERE recipe_id = @id ORDER BY sort_order",
            new { id = id.Value })).ToList();

        var hints = (await _connection.QueryAsync<HintRow>(
            "SELECT recipe_id AS RecipeId, text AS Text FROM recipe_hints WHERE recipe_id = @id ORDER BY sort_order",
            new { id = id.Value })).ToList();

        return BuildRecipe(row, steps, tags, ingredients, images, hints);
    }

    public async ValueTask<RecipeId> NextId()
    {
        if (_nextId < 0)
        {
            _nextId = await _connection.ExecuteScalarAsync<int>("SELECT COALESCE(MAX(id), 0) FROM recipes");
        }

        return new RecipeId(++_nextId);
    }

    public async ValueTask Save(Recipe recipe)
    {
        using var transaction = _connection.BeginTransaction();

        await _connection.ExecuteAsync(
            """
            INSERT OR REPLACE INTO recipes (id, name, description, author_id, difficulty, portions)
            VALUES (@Id, @Name, @Description, @AuthorId, @Difficulty, @Portions)
            """,
            new
            {
                Id = recipe.Id.Value,
                Name = recipe.Name.Value,
                Description = recipe.Description.Value,
                AuthorId = recipe.AuthorId.Value,
                Difficulty = (int)recipe.Difficulty,
                recipe.Portions,
            },
            transaction);

        if (recipe.PreparationSteps.Count > 0)
        {
            await _connection.ExecuteAsync(
                "INSERT INTO recipe_steps (recipe_id, sort_order, text) VALUES (@RecipeId, @SortOrder, @Text)",
                recipe.PreparationSteps.Select(s => new { RecipeId = recipe.Id.Value, s.SortOrder, s.Text }),
                transaction);
        }

        if (recipe.Tags.Count > 0)
        {
            await _connection.ExecuteAsync(
                "INSERT INTO recipe_tags (recipe_id, tag) VALUES (@RecipeId, @Tag)",
                recipe.Tags.Select(t => new { RecipeId = recipe.Id.Value, Tag = t.Value }),
                transaction);
        }

        if (recipe.Ingredients.Count > 0)
        {
            await _connection.ExecuteAsync(
                "INSERT INTO recipe_ingredients (recipe_id, sort_order, ingredient_id, amount, unit_abbreviation) VALUES (@RecipeId, @SortOrder, @IngredientId, @Amount, @UnitAbbreviation)",
                recipe.Ingredients.Select((ri, i) => new
                {
                    RecipeId = recipe.Id.Value,
                    SortOrder = i + 1,
                    IngredientId = ri.IngredientId.Value,
                    ri.Amount,
                    UnitAbbreviation = ri.Unit.Abbreviation,
                }),
                transaction);
        }

        if (recipe.Images.Count > 0)
        {
            await _connection.ExecuteAsync(
                "INSERT INTO recipe_images (recipe_id, sort_order, url) VALUES (@RecipeId, @SortOrder, @Url)",
                recipe.Images.Select((url, i) => new { RecipeId = recipe.Id.Value, SortOrder = i + 1, Url = url }),
                transaction);
        }

        if (recipe.Hints.Count > 0)
        {
            await _connection.ExecuteAsync(
                "INSERT INTO recipe_hints (recipe_id, sort_order, text) VALUES (@RecipeId, @SortOrder, @Text)",
                recipe.Hints.Select((h, i) => new { RecipeId = recipe.Id.Value, SortOrder = i + 1, h.Text }),
                transaction);
        }

        transaction.Commit();
    }

    public async ValueTask Delete(RecipeId id)
    {
        await _connection.ExecuteAsync("DELETE FROM recipes WHERE id = @id", new { id = id.Value });
    }

    private static Recipe BuildRecipe(
        RecipeRow row,
        IReadOnlyList<StepRow> steps,
        IReadOnlyList<TagRow> tags,
        IReadOnlyList<IngredientRow> ingredients,
        IReadOnlyList<ImageRow> images,
        IReadOnlyList<HintRow> hints) =>
        Recipe.Create(
            new RecipeId((int)row.Id),
            new RecipeName(row.Name),
            new RecipeDescription(row.Description),
            steps.Select(s => new PreparationStep((int)s.SortOrder, s.Text)).ToList(),
            new UserId((int)row.AuthorId),
            (Difficulty)row.Difficulty,
            tags.Select(t => new RecipeTag(t.Tag)).ToList(),
            (int)row.Portions,
            ingredients.Select(i => new RecipeIngredient(
                new IngredientId((int)i.IngredientId),
                i.Amount,
                ParseUnit(i.UnitAbbreviation))).ToList(),
            images.Select(i => i.Url).ToList(),
            hints.Select(h => new RecipeHint(h.Text)).ToList());

    private static Unit ParseUnit(string abbreviation) => abbreviation switch
    {
        "g" => WeightUnit.Gram,
        "kg" => WeightUnit.Kilogram,
        "ml" => VolumeUnit.Milliliter,
        "dl" => VolumeUnit.Deciliter,
        "l" => VolumeUnit.Liter,
        "EL" => VolumeUnit.Tablespoon,
        "TL" => VolumeUnit.Teaspoon,
        "Stück" => PieceUnit.Piece,
        "Prise" => PieceUnit.Pinch,
        _ => throw new InvalidOperationException($"Unknown unit abbreviation: {abbreviation}"),
    };

    private sealed record RecipeRow(long Id, string Name, string Description, long AuthorId, long Difficulty, long Portions);

    private sealed record StepRow(long RecipeId, long SortOrder, string Text);

    private sealed record TagRow(long RecipeId, string Tag);

    private sealed record IngredientRow(long RecipeId, long SortOrder, long IngredientId, decimal Amount, string UnitAbbreviation);

    private sealed record ImageRow(long RecipeId, string Url);

    private sealed record HintRow(long RecipeId, string Text);
}
