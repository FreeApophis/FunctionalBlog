namespace FunctionalBlog.Application.Translations;

public interface ITranslationRepository
{
    ValueTask<IReadOnlyList<Translation>> All();

    ValueTask Save(string key, string language, string? variant, string text);
}
