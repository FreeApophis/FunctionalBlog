# Bug report: `IndexWriter.Commit()` throws `FileNotFoundException: Segment file is missing` after many incremental commit cycles

## Environment

| | |
|---|---|
| LeanCorpus | 1.4.0 **and 1.4.1** — both reproduce (NuGet, `Rowles.LeanCorpus`) |
| TargetFramework | `net10.0` |
| Runtime | .NET 10 |
| OS | Windows 11 (10.0.26200) |
| Directory | `MMapDirectory` (on-disk) |
| `IndexWriterConfig` | defaults (`MergeThreshold = 10`, `DeletionPolicy = KeepLatestCommitPolicy`, `MaxBufferedDocs = 10000`, `RamBufferSizeMB = 256`, `DurableCommits = true`) |

## Summary

When a single long-lived `IndexWriter` is used in a **commit-per-write** pattern (each
`UpdateDocument` / `DeleteDocuments` followed by `Commit()`) **while a `SearcherManager` over the same
directory is being refreshed and searched**, after a few hundred commit cycles a `Commit()` call
throws:

```
System.IO.FileNotFoundException: Segment file is missing: '.../full-text-search/seg_557.seg'.
```

The failure originates in commit-time stats accumulation, which scans an `SegmentInfo` whose backing
files have already been deleted (by a merge / the deletion policy). Once it starts, **every
subsequent `Commit()` on that writer keeps failing** — the index is effectively wedged until the
process is restarted and the index rebuilt from scratch.

## Stack trace

```
System.IO.FileNotFoundException: Segment file is missing: '.\data\full-text-search\seg_557.seg'.
File name: '.\data\full-text-search\seg_557.seg'
   at Rowles.LeanCorpus.Index.Segment.SegmentReader.ValidateExistingFile(String path)
   at Rowles.LeanCorpus.Index.Segment.SegmentReader.ValidateSegmentFiles(String basePath, Int32 docCount)
   at Rowles.LeanCorpus.Index.Segment.SegmentReader..ctor(MMapDirectory directory, SegmentInfo info)
   at Rowles.LeanCorpus.Index.Indexer.IndexWriter.AccumulateSegmentStatsByScan(SegmentInfo segment, Dictionary`2 fieldLengthSums, Dictionary`2 fieldDocCounts, Int32& totalDocCount, Int32& liveDocCount)
   at Rowles.LeanCorpus.Index.Indexer.IndexWriter.WriteCommitStats(Int32 generation)
   at Rowles.LeanCorpus.Index.Indexer.IndexWriter.CommitCore()
   at Rowles.LeanCorpus.Index.Indexer.IndexWriter.CommitWithLocks()
   at Rowles.LeanCorpus.Index.Indexer.IndexWriter.Commit()
```

(The same class of failure was also seen earlier as an `IOException` while *flushing* a new segment —
`IndexOutput..ctor(String filePath, Boolean durable)` from `SegmentFlusher.Flush` — when a freshly
chosen segment ordinal collided with a still-mapped file. Both point at segment-lifecycle
bookkeeping under churn.)

## Steps to reproduce

The trigger is **concurrent reads via a `SearcherManager` interleaved with per-operation commits**:
the searcher leases pin segments so the merger cannot reclaim them in step with the deletion policy,
and a later `Commit()` scans a segment whose files have already been removed.

1. Open one `IndexWriter` **and a `SearcherManager`** over the same `MMapDirectory` (default
   `IndexWriterConfig`).
2. In a loop: upsert a document with a **distinct** `_key` (so the index keeps growing), occasionally
   delete an older doc, `Commit()`, `MaybeRefresh()`, and **every few iterations run a search via
   `UsingSearcher`**.
3. Within a few hundred iterations a `Commit()` throws the `FileNotFoundException` above.

Without the `SearcherManager`/searches, the merger reclaims old segments freely and the bug does
**not** appear even after 4000 commits (segment count stays ~10–18). Adding the interleaved searches
makes it fail reliably within ~100–600 iterations.

### Minimal, self-contained repro (.NET 10 file-based app)

Save as `repro.cs` and run `dotnet run repro.cs`:

```csharp
#:package LeanCorpus@1.4.1
using Rowles.LeanCorpus.Document;
using Rowles.LeanCorpus.Document.Fields;
using Rowles.LeanCorpus.Index.Indexer;
using Rowles.LeanCorpus.Search.Queries;
using Rowles.LeanCorpus.Search.Searcher;
using Rowles.LeanCorpus.Store;

var path = Path.Combine(Path.GetTempPath(), "lcrepro_" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(path);

using var dir = new MMapDirectory(path);
using var writer = new IndexWriter(dir, new IndexWriterConfig());
using var manager = new SearcherManager(dir, null);

for (var i = 0; i < 3000; i++)
{
    var key = $"item_{i}";                                  // distinct keys -> index grows
    var doc = new LeanDocument();
    doc.Add(new StringField("_key", key, stored: false));
    doc.Add(new TextField("body", $"content number {i} lorem ipsum", stored: true, boost: 1.0f));

    writer.UpdateDocument("_key", key, doc);
    if (i % 5 == 0 && i > 20)
        writer.DeleteDocuments(new TermQuery("_key", $"item_{i - 17}"));
    writer.Commit();
    manager.MaybeRefresh();
    if (i % 3 == 0)
        manager.UsingSearcher(s => s.Search(new TermQuery("body", "lorem"), 10).ScoreDocs.Length);
}
Console.WriteLine("completed with no error");   // <-- not reached; Commit() throws first
```

### Observed

Reliably throws (the failing segment ordinal varies run-to-run):

| Version | Run 1 | Run 2 |
|---|---|---|
| 1.4.0 | `seg_381.seg` missing | `seg_381.seg` missing |
| 1.4.1 | `seg_277.seg` missing | `seg_608.seg` missing |

i.e. **1.4.1 does not fix it.**

### Variations tried — all still fail

Following the `docs/tutorials/concurrency` guidance, none of these avoid the crash (all reproduce
the same `FileNotFoundException` within a few hundred iterations):

| Variation | Result |
|---|---|
| `DeletionPolicy = new KeepLastNCommitsPolicy(maxCommits: 5)` (instead of default `KeepLatestCommitPolicy`) | still fails (`seg_289`) |
| Documented pattern: `new SearcherManager(dir, new SearcherManagerConfig { RefreshInterval = 200ms })` **+ batched commits** (every 50 ops) instead of commit-per-write, relying on the background refresh loop | still fails, even **earlier** (`seg_9`) |
| **No `SearcherManager` at all** (writer + commit-per-write only, 4000 iterations) | **passes** — segment count stays ~10–18 |

The last row is the key isolator: the failure requires a **live `SearcherManager` reading the
directory concurrently with the merging `IndexWriter`**. It is independent of commit cadence, the
deletion policy, and the searcher config — which suggests a merge deletes a segment that the
commit-stats scan (and/or a concurrent searcher) still references, rather than anything governed by
the documented commit/retention knobs.

### The built-in recovery tooling does not recover from this

Following `docs/tutorials/index-management/03-validation-recovery.md`, on each crash we dispose the
writer/searcher/directory and call `IndexRecovery.RecoverLatestCommit(path, cleanupOrphans: true)`,
then reopen. Observed:

- `RecoverLatestCommit` returns a **non-null** commit, but `IndexValidator.Check(dir).IsHealthy` is
  **`False`** immediately afterwards (before any further writes) — the recovered index is still
  reported unhealthy.
- Indexing **re-crashes within a few operations** every time (≈60 crash→recover cycles over a few
  hundred iterations). Recovery does bound the damage — search keeps returning hits and the segment
  count stays small (orphan cleanup works) — but it never reaches a healthy, stable state.

So the corruption is not cleanly recoverable in-process even with the library's own
`IndexRecovery` / `IndexValidator` APIs.

## Expected behaviour

`Commit()` either succeeds or fails transactionally without leaving the writer permanently broken.
Commit-time stats accumulation should not read segment files that the same commit/merge cycle has
already deleted; the live segment set used for stats should be consistent with what is actually on
disk.

## Actual behaviour

`Commit()` throws `FileNotFoundException` for a segment file that is referenced by an in-memory
`SegmentInfo` but no longer exists on disk. After the first failure the writer cannot commit again,
so all further writes silently fail to persist.

## Impact

High for any "live index" usage that commits per change. There is no in-process recovery: the
writer stays wedged, and (see note 3 below) the index cannot be cleanly rebuilt in-process either,
so a full process restart is required to restore search.

## Workaround

We currently wrap every live write/commit in `try { ... } catch (IOException) { /* swallow */ }` so
the corruption doesn't surface to callers, treating the index as a best-effort cache that is fully
rebuilt from the source database on process start. This avoids user-facing 500s but means live
search silently stops updating after the first corruption until the next restart.

---

## Additional observations (possibly separate issues / questions)

**2. `SearcherManager.MaybeRefresh()` does not surface newly committed documents on a large index.**
With a large index (~200 segments), after `writer.UpdateDocument(...)` + `writer.Commit()` +
`manager.MaybeRefresh()`, a subsequent `manager.UsingSearcher(...)` search does **not** find the
just-committed document (it stays invisible indefinitely, not just briefly). The same code works as
expected on a small index, and even the initial post-`RebuildAsync` searcher only reflected the
commit after a short delay. Is `MaybeRefresh()` time-/contention-throttled, and is there a
recommended way to force a refresh that is guaranteed to observe the latest commit (e.g. a blocking
`Refresh()`)?

**3. `MMapDirectory` appears to retain file handles/maps after `Dispose()` on Windows.**
Disposing the `SearcherManager`, `IndexWriter` and `MMapDirectory`, then deleting the directory's
files and re-opening a new `MMapDirectory` at the same path **in the same process** on Windows does
not reliably produce a clean index: `File.Delete` succeeds without error but the files linger
(delete-pending, because mappings are still open), and the re-opened index ends up empty/corrupt and
the segment files accumulate. This makes an in-process "rebuild from scratch" impossible as a
recovery path. Is there a supported way to fully release a directory and rebuild in place, or should
the directory only ever be cleared from a fresh process?

**4. Thread-safety.** `IndexWriter`'s `UpdateDocument` / `DeleteDocuments` / `Commit` do not appear
safe for concurrent calls (separate `AddDocumentLockFree` / `AddDocumentsConcurrent` methods exist).
A short note in the docs on the intended concurrency model (single-writer; `SearcherManager` safe for
concurrent reads) would help — we had to serialise all writer access behind our own lock.
