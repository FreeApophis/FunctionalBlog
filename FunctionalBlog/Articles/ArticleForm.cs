namespace FunctionalBlog.Articles;

public static class ArticleForm
{
    public static DecodedArticleForm Decode(Request request)
    {
        var title = request.Form.GetValueOrNone("title").Match(none: string.Empty, some: Trim);
        var teaser = request.Form.GetValueOrNone("teaser").Match(none: string.Empty, some: Trim);
        var text = request.Form.GetValueOrNone("text").Match(none: string.Empty, some: Trim);

        var errors = new List<string>();

        if (title.Length < 3)
        {
            errors.Add("article.error.title_too_short");
        }

        if (teaser.Length < 10)
        {
            errors.Add("article.error.teaser_too_short");
        }

        if (text.Length < 10)
        {
            errors.Add("article.error.text_too_short");
        }

        return new DecodedArticleForm(
            IsValid: errors.Count == 0,
            Errors: errors,
            Title: title,
            Teaser: teaser,
            Text: text);
    }

    private static string Trim(string arg) => arg.Trim();
}
