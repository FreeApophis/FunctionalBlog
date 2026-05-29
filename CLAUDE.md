# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build                  # Debug build
dotnet build -c Release       # Release build
dotnet run                    # Run (HTTPS: localhost:65243, HTTP: localhost:65244)
dotnet clean                  # Clean build artifacts
dotnet test                   # Run the xunit test project
```

Tests live in `FunctionalBlog.Test/` (xunit). There is no linting configuration. NuGet package versions are managed centrally in `Directory.Packages.props` (CPM is enabled).

## Architecture

The codebase demonstrates functional programming principles in .NET 10. The core abstraction is a curried, reader-style pipeline expressed with delegates:

```
Effect<T>  = Func<Env, ValueTask<T>>           // Effect.cs
App        = Func<Request, Effect<Response>>   // App.cs
Middleware = Func<App, App>                    // Middleware.cs
```

A handler is invoked as `app(request)(env)` — request first, then environment. `Functional.Compose(params Middleware[])` folds middlewares right-to-left over a `NotFound` terminator to produce the final `App`. In `Program.cs` the pipeline is `Recover → RequestLogging → Router`.

### Key types

- **`Env`** (`Env.cs`) — dependency injection record carrying `IArticleRepository`, `IClock`, and `ILog`. Passed through every handler.
- **`Request`** / **`Response`** (`Request.cs`, `Response.cs`) — domain-level HTTP abstractions. `Response` has `Html`, `Text`, `Redirect`, and `NotFound` factory methods.
- **`Router`** (`Router.cs`) — a `Middleware` that pattern-matches on `(method, path)` and dispatches to `BlogHandlers`; falls through to `Response.NotFound()`.
- **`BlogHandlers`** (`BlogHandlers.cs`) — `App`-valued handlers (Index, NewArticleForm, CreateArticle, ShowArticle) that take a `Request` and return an `Effect<Response>`.
- **`Middlewares`** (`Middlewares.cs`) — `Recover` (catches exceptions, logs, returns a 500 HTML page) and `RequestLogging` (times the inner app and logs method/path/status/elapsed).
- **`Article` / `ArticleId` / `ArticleTitle` / `ArticleText` / `ArticleForm` / `DecodedArticleForm`** — domain model and form decoding/validation, each in its own file.
- **`InMemoryArticleRepository`** (`InMemoryArticleRepository.cs`) — the only storage implementation; no database. Pre-seeded with one sample article.
- **`BlogViews`** / **`Layout`** / **`Html`** — server-rendered HTML generation with XSS protection via `WebUtility.HtmlEncode`.
- **`HttpAdapter`** (`HttpAdapter.cs`) — adapts ASP.NET Core's `HttpContext` to/from the domain `Request`/`Response`.
- **`SystemClock`** / **`ConsoleLog`** — concrete `IClock` / `ILog` implementations wired up in `Program.cs`.

### UI language

All user-facing text (form validation messages, article content, page labels) is in **German**.

### File layout

One public type per file; no namespaces (relies on the project's implicit global namespace and `ImplicitUsings`). Files group by role:

1. Functional core: `Effect.cs`, `App.cs`, `Middleware.cs`, `Functional.cs`
2. HTTP model: `Request.cs`, `Response.cs`, `Env.cs`
3. Pipeline: `Middlewares.cs`, `Router.cs`
4. Application: `BlogHandlers.cs`
5. Domain: `Article.cs`, `ArticleId.cs`, `ArticleTitle.cs`, `ArticleText.cs`, `ArticleForm.cs`, `DecodedArticleForm.cs`
6. Storage: `IArticleRepository.cs`, `InMemoryArticleRepository.cs`
7. Views: `BlogViews.cs`, `Layout.cs`, `Html.cs`
8. Infrastructure: `IClock.cs`, `SystemClock.cs`, `ILog.cs`, `ConsoleLog.cs`, `HttpAdapter.cs`
9. Bootstrap: `Program.cs`
