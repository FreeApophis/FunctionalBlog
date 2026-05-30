using System.Collections.Concurrent;

public sealed class InMemoryArticleRepository : IArticleRepository
{
    private readonly ConcurrentDictionary<int, Article> _articles = new();
    private int _nextId = 3;

    public InMemoryArticleRepository()
    {
        _articles[1] = Article.Create(
            new ArticleId(1),
            new ArticleTitle("Hallo funktionales Blog"),
            new ArticleText("Dies ist der erste Artikel. Die Anwendung ist absichtlich klein, aber funktional aufgebaut."),
            DateTimeOffset.UtcNow);

        _articles[2] = Article.Create(
            new ArticleId(2),
            new ArticleTitle("Macarons selbst backen"),
            new ArticleText(" Jeder hat sie zumindest schon gesehen: die hübschen bunten Macarons. Neben ihrem tollen Aussehen schmecken sie auch fantastisch. Und wer nicht für diese kleinen teuren Gebäcke zahlen möchte, der kann versuchen sie selbst zu machen.\r\n\r\nJeder kennt sie: die bunten kleinen Eiweissgebäcke von Sprüngli oder Ladurée und vielen anderen Macaronsherstellern. Sie alle haben folgendes gemeinsam: sie sind bunt, sie sind lecker und sie sind teuer.\r\n\r\nWeil die Herstellung von Macarons eine sehr arbeitsaufwändige ist, trauen sich viele nicht daran. An der Stelle muss ich zugeben, auch ich hab zahlreiche Macarons-Versuche gestartet, einige missglückten, andere konnte man zwar essen, aber sie waren nicht wie sie sein sollten: zartschmelzend. In der Zwischenzeit habe ich meine eigenen Creme-Kreationen und möchte nun mein Rezept teilen.\r\n\r\nAber aus was bestehen diese kleinen Baisers eigentlich? Die Struktur eines Macarons gleicht der eines Doppelkekses: Keks-Füllung-Keks. Der Keks besteht dabei vor allem aus Eiweiss, Puderzucker und gemahlenen weissen Mandeln. Ziel sollte sein, dass das Mandelbaiser unter einer hauchdünnen, glatten Kruste weich, feucht, schließlich cremig und schnell zergehend im Mund ist. Die Füllung besteht in der Regel aus einer Buttercreme, einer Ganache oder Konfitüre. Sie bieten den Vorteil, dass man seiner Fantasie freien Lauf lassen kann. Mein neustes Buttercreme-Experiment: eine Erdbeer-Wasabi-Buttercreme. Ihr seht, man kann wirklich das zusammenmischen, was man möchte.\r\n\r\nWorauf kommt es denn beim Backen an? Macarons gelten als sehr schwierig zu backen. Dem kann ich zu einem gewissen Grad durchaus zustimmen. Aber um an dieser Stelle zu beruhigen: kein Meister ist vom Himmel gefallen und ich bin nur eine Hobbybäckerin und kein Macaron-Profi. Und dennoch kann man essbare, gutaussehende, bunte Macarons ganz von von alleine zu Hause backen.\r\n\r\nWichtig ist vor allem:\r\n\r\n\r\n    Die Zutaten aufs Gramm genau abwägen.\r\n\r\n    Die Backtemperatur sowie Backdauer genau im Auge behalten.\r\n\r\n    Und die Ruhezeiten beachten\r\n\r\n\r\nKeine Angst, das klingt erstmal schwerer als es ist. Mit ein bisschen Übung, die brauchte ich auch, und viel Zeit sind Macarons nur halb so schwer wie alle immer sagen. Man sollte einfach viel Zeit einplanen für die eigene Macaron-Herstellung.."),
            DateTimeOffset.UtcNow);
    }

    public ValueTask<IReadOnlyList<Article>> All()
    {
        var articles = _articles.Values
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return ValueTask.FromResult<IReadOnlyList<Article>>(articles);
    }

    public ValueTask<Article?> Find(ArticleId id)
    {
        _articles.TryGetValue(id.Value, out var article);
        return ValueTask.FromResult(article);
    }

    public ValueTask<ArticleId> NextId()
    {
        var id = Interlocked.Increment(ref _nextId);
        return ValueTask.FromResult(new ArticleId(id));
    }

    public ValueTask Save(Article article)
    {
        _articles[article.Id.Value] = article;
        return ValueTask.CompletedTask;
    }
}
