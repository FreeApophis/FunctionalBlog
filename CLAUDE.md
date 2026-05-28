# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build                  # Debug build
dotnet build -c Release       # Release build
dotnet run                    # Run (HTTPS: localhost:65243, HTTP: localhost:65244)
dotnet clean                  # Clean build artifacts
```

There are no test projects or linting configuration.

## Architecture

The entire application lives in a single file: `FunctionalBlog/Program.cs` (~542 lines). There are no external NuGet dependencies beyond the ASP.NET Core base packages.

The codebase demonstrates functional programming principles in .NET 10. The core abstraction is a middleware pipeline:

```
Effect = Func<Env, Request, ValueTask<Response>>
App = Effect
Middleware = Func<App, App>
```

`Functional.Compose()` chains middleware (Recover → RequestLogging → Router) into an App, which is registered as a single ASP.NET Core request delegate via `HttpAdapter`.

### Key types

- **`Env`** — dependency injection record carrying `IArticleRepository`, `IClock`, and `ILog`. Passed through every handler.
- **`Request`** / **`Response`** — domain-level HTTP abstractions. `Response` has `Html`, `Text`, and `Redirect` factory methods.
- **`Router`** — pattern-matches on `(method, path segments)` and dispatches to `BlogHandlers`.
- **`BlogHandlers`** — pure functions (Index, NewArticleForm, CreateArticle, ShowArticle) that take `Env + Request` and return `Response`.
- **`InMemoryArticleRepository`** — the only storage implementation; no database. Pre-seeded with one sample article.
- **`BlogViews`** / **`Layout`** / **`Html`** — server-rendered HTML generation with XSS protection via `WebUtility.HtmlEncode`.

### UI language

All user-facing text (form validation messages, article content, page labels) is in **German**.

### Layering order in Program.cs

1. Functional type definitions (Effect, Middleware, Compose)
2. Request/Response/Env models
3. Middleware (Recover, RequestLogging)
4. Router
5. BlogHandlers (application logic)
6. Domain models (Article, ArticleId, ArticleTitle, ArticleText, ArticleForm)
7. InMemoryArticleRepository
8. Views (BlogViews, Layout, Html)
9. Infrastructure (IClock, ILog, HttpAdapter)
10. ASP.NET Core bootstrap (app.Run)
