using System.Collections.Concurrent;

namespace FunctionalBlog.DataAccess.Tags;

// A simple seedable tag dictionary for tests. Production uses the SQLite repository, which
// reads the normalized `tags` table.
public sealed class InMemoryTagRepository : ITagRepository
{
    private readonly ConcurrentDictionary<string, Tag> _tags = new();

    public InMemoryTagRepository(params Tag[] seed)
    {
        foreach (var tag in seed)
        {
            _tags[tag.Slug] = tag;
        }
    }

    public ValueTask<Option<Tag>> FindBySlug(string slug) =>
        ValueTask.FromResult(_tags.GetValueOrNone(slug));
}
