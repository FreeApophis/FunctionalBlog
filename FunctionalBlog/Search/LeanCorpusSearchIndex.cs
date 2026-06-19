using System.Text.RegularExpressions;
using Rowles.LeanCorpus.Analysis.Analysers;
using Rowles.LeanCorpus.Document;
using Rowles.LeanCorpus.Document.Fields;
using Rowles.LeanCorpus.Index.Indexer;
using Rowles.LeanCorpus.Search;
using Rowles.LeanCorpus.Search.Highlighting;
using Rowles.LeanCorpus.Search.Parsing;
using Rowles.LeanCorpus.Search.Queries;
using Rowles.LeanCorpus.Search.Searcher;
using Rowles.LeanCorpus.Search.Suggestions;
using Rowles.LeanCorpus.Store;

namespace FunctionalBlog.Search;

public sealed class LeanCorpusSearchIndex : ISearchIndex, IDisposable
{
    private readonly MMapDirectory _directory;

    private readonly IndexWriter _writer;

    private readonly SearcherManager _manager;

    private readonly IAnalyser _analyser;

    private readonly Highlighter _highlighter;

    public LeanCorpusSearchIndex(string indexPath)
    {
        PrepareCleanDirectory(indexPath);
        _directory = new MMapDirectory(indexPath);
        _writer = new IndexWriter(_directory, new IndexWriterConfig());
        _manager = new SearcherManager(_directory, null);
        _analyser = new StandardAnalyser(40, []);
        _highlighter = new Highlighter("<mark>", "</mark>", _analyser);
    }

    public void IndexArticle(Article article)
    {
        _writer.UpdateDocument("_key", $"article_{article.Id.Value}", BuildArticleDocument(article));
        _writer.Commit();
        _manager.MaybeRefresh();
    }

    public void IndexRecipe(Recipe recipe)
    {
        _writer.UpdateDocument("_key", $"recipe_{recipe.Id.Value}", BuildRecipeDocument(recipe));
        _writer.Commit();
        _manager.MaybeRefresh();
    }

    public void IndexIngredient(Ingredient ingredient)
    {
        _writer.UpdateDocument("_key", $"ingredient_{ingredient.Id.Value}", BuildIngredientDocument(ingredient));
        _writer.Commit();
        _manager.MaybeRefresh();
    }

    public void DeleteDocument(string type, int id)
    {
        _writer.DeleteDocuments(new TermQuery("_key", $"{type}_{id}"));
        _writer.Commit();
        _manager.MaybeRefresh();
    }

    public IReadOnlyList<SearchResult> Search(string query, int topN = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var parsedQuery = ParseQuery(query);

        return _manager.UsingSearcher(searcher =>
        {
            var topDocs = searcher.Search(parsedQuery, topN);
            var queryTerms = Highlighter.ExtractTerms(parsedQuery);

            return (IReadOnlyList<SearchResult>)topDocs.ScoreDocs
                .Select(hit =>
                {
                    var fields = searcher.GetStoredFields(hit.DocId);
                    var type = fields["type"]?[0] ?? string.Empty;
                    var id = int.Parse(fields["id"]?[0] ?? "0");
                    var title = fields["title"]?[0] ?? string.Empty;
                    var body = fields["body"]?[0] ?? string.Empty;
                    var snippet = _highlighter.GetBestFragment(body, queryTerms, 200);
                    return new SearchResult(type, id, title, snippet, hit.Score);
                })
                .ToList();
        });
    }

    public IReadOnlyList<string> Suggestions(string query)
    {
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return _manager.UsingSearcher(searcher =>
            (IReadOnlyList<string>)terms
                .SelectMany(term =>
                    DidYouMeanSuggester.Suggest(searcher, "title", term, maxEdits: 2, topN: 3)
                        .Concat(DidYouMeanSuggester.Suggest(searcher, "body", term, maxEdits: 2, topN: 3)))
                .OrderByDescending(s => s.Score)
                .Take(5)
                .Select(s => s.Term)
                .Distinct()
                .ToList());
    }

    public async ValueTask RebuildAsync(
        IArticleRepository articles,
        IRecipeRepository recipes,
        IIngredientRepository ingredients)
    {
        _writer.DeleteDocuments(new TermQuery("type", "article"));
        _writer.DeleteDocuments(new TermQuery("type", "recipe"));
        _writer.DeleteDocuments(new TermQuery("type", "ingredient"));

        foreach (var article in await articles.All())
        {
            _writer.UpdateDocument("_key", $"article_{article.Id.Value}", BuildArticleDocument(article));
        }

        foreach (var recipe in await recipes.All())
        {
            _writer.UpdateDocument("_key", $"recipe_{recipe.Id.Value}", BuildRecipeDocument(recipe));
        }

        foreach (var ingredient in await ingredients.All())
        {
            _writer.UpdateDocument("_key", $"ingredient_{ingredient.Id.Value}", BuildIngredientDocument(ingredient));
        }

        _writer.Commit();
        _manager.MaybeRefresh();
    }

    public void Dispose()
    {
        _manager.Dispose();
        _writer.Dispose();
        _directory.Dispose();
    }

    // The on-disk index is a rebuildable cache: Program.Main repopulates it from the
    // database via RebuildAsync on every startup, so nothing persisted here is ever read.
    // Starting from a clean directory stops superseded segment files from accumulating
    // across restarts (which grew the production index to seg_186 and triggered a
    // file-lock crash on startup). It also surfaces a leftover/second instance early:
    // the delete fails fast if another process still holds the index files memory-mapped.
    private static void PrepareCleanDirectory(string indexPath)
    {
        Directory.CreateDirectory(indexPath);

        foreach (var file in Directory.EnumerateFiles(indexPath))
        {
            try
            {
                File.Delete(file);
            }
            catch (IOException ex)
            {
                throw new IOException(
                    $"Could not clear the search index at '{indexPath}'. Another FunctionalBlog instance may still be running and holding the index files.",
                    ex);
            }
        }
    }

    private Query ParseQuery(string userInput)
    {
        try
        {
            var bodyQuery = new QueryParser("body", _analyser).Parse(userInput);
            var titleQuery = new QueryParser("title", _analyser).Parse(userInput);
            return bodyQuery.Or(titleQuery);
        }
        catch (QueryParseException)
        {
            return new QueryParser("body", _analyser, lenient: true).Parse(Regex.Escape(userInput.Trim()));
        }
    }

    private static LeanDocument BuildArticleDocument(Article article) =>
        BuildDocument(
            "article",
            article.Id.Value,
            article.Title.Value,
            $"{article.Teaser.Value} {article.Text.Value}");

    private static LeanDocument BuildRecipeDocument(Recipe recipe)
    {
        var parts = new[]
        {
            recipe.Description.Value,
            string.Join(" ", recipe.Tags.Select(t => t.Value)),
            string.Join(" ", recipe.Hints.Select(h => h.Text)),
            string.Join(" ", recipe.PreparationSteps.Select(s => s.Text)),
        };
        return BuildDocument("recipe", recipe.Id.Value, recipe.Name.Value, string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p))));
    }

    private static LeanDocument BuildIngredientDocument(Ingredient ingredient) =>
        BuildDocument("ingredient", ingredient.Id.Value, ingredient.Name.Value, ingredient.Description);

    private static LeanDocument BuildDocument(string type, int id, string title, string body)
    {
        var doc = new LeanDocument();
        doc.Add(new StringField("_key", $"{type}_{id}", stored: false));
        doc.Add(new StringField("type", type, stored: true));
        doc.Add(new StringField("id", id.ToString(), stored: true));
        doc.Add(new TextField("title", title, stored: true, boost: 2.5f));
        doc.Add(new TextField("body", body, stored: true, boost: 1.0f));
        return doc;
    }
}
