namespace FunctionalBlog.Application.Tags;

public interface ITagRepository
{
    // Looks up a single tag by its case-folded slug (e.g. "suess"). None when no tag matches.
    ValueTask<Option<Tag>> FindBySlug(string slug);
}
