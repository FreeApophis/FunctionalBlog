public static class ArticleForm
{
    public static DecodedArticleForm Decode(Request request)
    {
        var title = request.Form.GetValueOrDefault("title", string.Empty).Trim();
        var text = request.Form.GetValueOrDefault("text", string.Empty).Trim();

        var errors = new List<string>();

        if (title.Length < 3)
        {
            errors.Add("Der Titel muss mindestens 3 Zeichen lang sein.");
        }

        if (text.Length < 10)
        {
            errors.Add("Der Text muss mindestens 10 Zeichen lang sein.");
        }

        return new DecodedArticleForm(
            IsValid: errors.Count == 0,
            Errors: errors,
            Title: title,
            Text: text);
    }
}
