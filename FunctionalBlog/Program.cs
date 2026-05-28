var builder = WebApplication.CreateBuilder(args);
var web = builder.Build();

var env = new Env(
    Articles: new InMemoryArticleRepository(),
    Clock: new SystemClock(),
    Log: new ConsoleLog()
);

App app = Functional.Compose(
    Middlewares.Recover,
    Middlewares.RequestLogging,
    Router.Create()
);

web.Run(async http =>
{
    var request = await HttpAdapter.ToRequest(http);
    var response = await app(request)(env);
    await HttpAdapter.WriteResponse(http, response);
});

web.Run();
