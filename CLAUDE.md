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

## Workflow

This project follows **Test-Driven Development**. For every change to production code:

1. Write a failing test in `FunctionalBlog.Test/` that captures the desired behavior.
2. Run `dotnet test` and confirm it fails for the expected reason.
3. Write the minimum production code in the project that owns the change (`FunctionalBlog.Domain`, `FunctionalBlog.Application`, `FunctionalBlog.DataAccess`, or `FunctionalBlog`) to make it pass.
4. Run `dotnet test` again and confirm all tests pass.
5. Refactor while keeping tests green.

Do not add production code that is not driven by a failing test. Do not skip step 2 — a test that has never been red proves nothing.

When adding a new `IArticleRepository` implementation, derive its tests from `ArticleRepositoryContract` (in `FunctionalBlog.Test/`) so the contract is enforced uniformly across impls.

## Architecture

The solution is split into five projects with a strict dependency direction (Domain ← Application ← DataAccess ← Web ← Test):

- **`FunctionalBlog.Domain`** — entities and value objects. No project references. Pure domain.
- **`FunctionalBlog.Application`** — repository contracts. References Domain.
- **`FunctionalBlog.DataAccess`** — repository implementations. References Application. `InMemoryArticleRepository` is the only impl today; a SQL/EF impl will join it later.
- **`FunctionalBlog`** (Web) — handlers, views, the functional pipeline, and the ASP.NET bootstrap. References Application and DataAccess.
- **`FunctionalBlog.Test`** — xunit. References Web; sees Domain/Application/DataAccess transitively.

The Web project demonstrates functional programming principles in .NET 10. The core abstraction is a curried, reader-style pipeline expressed with delegates:

```
Effect<T>  = Func<Env, ValueTask<T>>           // Effect.cs
App        = Func<Request, Effect<Response>>   // App.cs
Middleware = Func<App, App>                    // Middleware.cs
```

A handler is invoked as `app(request)(env)` — request first, then environment. `Functional.Compose(params Middleware[])` folds middlewares right-to-left over a `NotFound` terminator to produce the final `App`. In `Program.cs` the pipeline is `Recover → RequestLogging → Router`.

### Key types

- **`Env`** (Web) — dependency injection record carrying `IArticleRepository`, `IClock`, and `ILog`. Passed through every handler.
- **`Request`** / **`Response`** (Web) — HTTP abstractions. `Response` has `Html`, `Text`, `Css`, `Redirect`, and `NotFound` factory methods.
- **`Router`** (Web) — a `Middleware` that pattern-matches on `(method, path)` and dispatches to `BlogHandlers`/`StaticHandlers`; falls through to `Response.NotFound()`.
- **`BlogHandlers`** (Web) — `App`-valued handlers (Index, NewArticleForm, CreateArticle, ShowArticle).
- **`StaticHandlers`** (Web) — serves `/styles.css` from the embedded resource `FunctionalBlog.wwwroot.styles.css`. Content cached in a `Lazy<string>`.
- **`Middlewares`** (Web) — `Recover` (catches exceptions, logs, returns a 500 HTML page) and `RequestLogging` (times the inner app and logs method/path/status/elapsed).
- **`Article` / `ArticleId` / `ArticleTitle` / `ArticleText`** (Domain) — domain model.
- **`IArticleRepository`** (Application) — repository contract. Behavioral spec lives in `FunctionalBlog.Test/ArticleRepositoryContract.cs`.
- **`ArticleForm` / `DecodedArticleForm`** (Web) — HTTP form decoding/validation. Stays with the Web project because it's a presentation/transport concern, not a domain rule.
- **`InMemoryArticleRepository`** (DataAccess) — the only storage implementation today. Pre-seeded with one sample article on construction (a fixture quirk, not part of the contract).
- **`BlogViews`** / **`Layout`** / **`Html`** (Web) — server-rendered HTML with XSS protection via `WebUtility.HtmlEncode`. CSS lives in `wwwroot/styles.css`, embedded into the Web assembly.
- **`HttpAdapter`** (Web) — adapts ASP.NET Core's `HttpContext` to/from the domain `Request`/`Response`.
- **`SystemClock`** / **`ConsoleLog`** (Web) — concrete `IClock` / `ILog` implementations wired up in `Program.cs`.

### UI language

All user-facing text (form validation messages, article content, page labels) is in **German**.

### File layout

One public type per file; no namespaces (relies on the implicit global namespace and `ImplicitUsings`).

**`FunctionalBlog.Domain/`**: `Article.cs`, `ArticleId.cs`, `ArticleTitle.cs`, `ArticleText.cs`

**`FunctionalBlog.Application/`**: `IArticleRepository.cs`

**`FunctionalBlog.DataAccess/`**: `InMemoryArticleRepository.cs`

**`FunctionalBlog/`** (Web), grouped by role:

1. Functional core: `Effect.cs`, `App.cs`, `Middleware.cs`, `Functional.cs`
2. HTTP model: `Request.cs`, `Response.cs`, `Env.cs`
3. Pipeline: `Middlewares.cs`, `Router.cs`
4. Application: `BlogHandlers.cs`, `StaticHandlers.cs`
5. Form: `ArticleForm.cs`, `DecodedArticleForm.cs`
6. Views: `BlogViews.cs`, `Layout.cs`, `Html.cs`, `wwwroot/styles.css`
7. Infrastructure: `IClock.cs`, `SystemClock.cs`, `ILog.cs`, `ConsoleLog.cs`, `HttpAdapter.cs`
8. Bootstrap: `Program.cs`
