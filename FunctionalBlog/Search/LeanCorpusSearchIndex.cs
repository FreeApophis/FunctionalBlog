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
    // The underlying IndexWriter is not safe for concurrent use and the directory/writer/manager
    // are swapped wholesale on every rebuild, so all access is serialised through this gate.
    // A single lock is plenty for the traffic here; a ReaderWriterLockSlim would be the next step
    // if concurrent search throughput ever mattered.
    private readonly object _gate = new();

    private readonly string _indexPath;

    private readonly IAnalyser _analyser;

    private readonly Highlighter _highlighter;

    private readonly MMapDirectory _directory;

    private readonly IndexWriter _writer;

    private readonly SearcherManager _manager;

    public LeanCorpusSearchIndex(string indexPath)
    {
        _indexPath = indexPath;
        _analyser = new StandardAnalyser(40, []);
        _highlighter = new Highlighter("<mark>", "</mark>", _analyser);

        PrepareCleanDirectory(_indexPath);
        _directory = new MMapDirectory(_indexPath);
        _writer = new IndexWriter(_directory, new IndexWriterConfig());
        _manager = new SearcherManager(_directory, null);
    }

    public void IndexArticle(Article article) =>
        TryWrite(() => _writer.UpdateDocument("_key", $"article_{article.Id.Value}", BuildArticleDocument(article)));

    public void IndexRecipe(Recipe recipe) =>
        TryWrite(() => _writer.UpdateDocument("_key", $"recipe_{recipe.Id.Value}", BuildRecipeDocument(recipe)));

    public void IndexIngredient(Ingredient ingredient) =>
        TryWrite(() => _writer.UpdateDocument("_key", $"ingredient_{ingredient.Id.Value}", BuildIngredientDocument(ingredient)));

    public void IndexPage(Page page) =>
        TryWrite(() => _writer.UpdateDocument("_key", $"page_{page.Id.Value}", BuildPageDocument(page)));

    public void DeleteDocument(string type, int id) =>
        TryWrite(() => _writer.DeleteDocuments(new TermQuery("_key", $"{type}_{id}")));

    public IReadOnlyList<SearchResult> Search(string query, int topN = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var parsedQuery = ParseQuery(query);

        lock (_gate)
        {
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
    }

    public IReadOnlyList<string> Suggestions(string query)
    {
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        lock (_gate)
        {
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
    }

    // Re-syncs the index with the database (the single source of truth): drops every indexed
    // document and re-adds the current rows, healing any drift left by a swallowed live-write
    // failure. Documents are built up front from the async repositories, then applied to the writer
    // under the gate with no await held. (The directory is cleared only on a fresh process start in
    // the constructor — disposing and deleting a memory-mapped directory in-process is not reliable
    // on Windows, where the files linger delete-pending and corrupt a re-opened index.)
    public async ValueTask RebuildAsync(
        IArticleRepository articles,
        IRecipeRepository recipes,
        IIngredientRepository ingredients,
        IPageRepository pages)
    {
        var documents = new List<(string Key, LeanDocument Document)>();

        foreach (var article in await articles.All())
        {
            documents.Add(($"article_{article.Id.Value}", BuildArticleDocument(article)));
        }

        foreach (var recipe in await recipes.All())
        {
            documents.Add(($"recipe_{recipe.Id.Value}", BuildRecipeDocument(recipe)));
        }

        foreach (var ingredient in await ingredients.All())
        {
            documents.Add(($"ingredient_{ingredient.Id.Value}", BuildIngredientDocument(ingredient)));
        }

        foreach (var page in await pages.All())
        {
            documents.Add(($"page_{page.Id.Value}", BuildPageDocument(page)));
        }

        lock (_gate)
        {
            _writer.DeleteDocuments(new TermQuery("type", "article"));
            _writer.DeleteDocuments(new TermQuery("type", "recipe"));
            _writer.DeleteDocuments(new TermQuery("type", "ingredient"));
            _writer.DeleteDocuments(new TermQuery("type", "page"));

            foreach (var (key, document) in documents)
            {
                _writer.UpdateDocument("_key", key, document);
            }

            _writer.Commit();
            _manager.MaybeRefresh();
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _manager.Dispose();
            _writer.Dispose();
            _directory.Dispose();
        }
    }

    // A single live write + commit so new content is searchable within the request. The on-disk
    // index is a best-effort cache rebuilt from the database (RebuildAsync), so a transient segment
    // inconsistency on an incremental commit must never fail the user's create/update/delete — it
    // is healed by the next (periodic) rebuild.
    private void TryWrite(Action write)
    {
        lock (_gate)
        {
            try
            {
                write();
                _writer.Commit();
                _manager.MaybeRefresh();
            }
            catch (IOException)
            {
                // Best-effort: the database remains the source of truth; a later rebuild heals the index.
            }
        }
    }

    // Clears the index directory. Across restarts this stops superseded segment files from
    // accumulating (which grew the production index to seg_186 and caused a file-lock crash), and
    // it surfaces a leftover/second instance early: the delete fails fast if another process still
    // holds the files memory-mapped.
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

    private static LeanDocument BuildPageDocument(Page page) =>
        BuildDocument("page", page.Id.Value, page.Title.Value, page.Content.Value);

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
