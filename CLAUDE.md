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
3. Write the minimum production code in the project that owns the change (`FunctionalBlog.Domain`, `FunctionalBlog.Application`, `FunctionalBlog.DataAccess.InMemory`, `FunctionalBlog.DataAccess.Sqlite`, or `FunctionalBlog`) to make it pass.
4. Run `dotnet test` again and confirm all tests pass.
5. Refactor while keeping tests green.

Do not add production code that is not driven by a failing test. Do not skip step 2 — a test that has never been red proves nothing.

When adding a new `IArticleRepository` implementation, derive its tests from `ArticleRepositoryContract` (in `FunctionalBlog.Test/`) so the contract is enforced uniformly across impls.

## Architecture

The solution is split into six projects with a strict dependency direction (Domain ← Application ← DataAccess.* ← Web ← Test):

- **`FunctionalBlog.Domain`** — entities and value objects. No project references. Pure domain.
- **`FunctionalBlog.Application`** — repository contracts. References Domain.
- **`FunctionalBlog.DataAccess.InMemory`** — in-memory repository implementations for use in tests. References Application.
- **`FunctionalBlog.DataAccess.Sqlite`** — SQLite repository implementations for production, plus `Pbkdf2PasswordHasher` and `TranslationSeeder`. References Application. Uses Dapper and dbup-sqlite.
- **`FunctionalBlog`** (Web) — handlers, views, the functional pipeline, and the ASP.NET bootstrap. References Application and DataAccess.Sqlite.
- **`FunctionalBlog.Test`** — xunit. References Web (gets DataAccess.Sqlite transitively) and DataAccess.InMemory directly.

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
- **`InMemoryArticleRepository`** (DataAccess.InMemory) — in-memory storage for tests. Pre-seeded with one sample article on construction (a fixture quirk, not part of the contract).
- **`SqliteArticleRepository`** (DataAccess.Sqlite) — production SQLite storage using Dapper.
- **`BlogViews`** / **`Layout`** / **`Html`** (Web) — server-rendered HTML with XSS protection via `WebUtility.HtmlEncode`. CSS lives in `wwwroot/styles.css`, embedded into the Web assembly.
- **`HttpAdapter`** (Web) — adapts ASP.NET Core's `HttpContext` to/from the domain `Request`/`Response`.
- **`SystemClock`** / **`ConsoleLog`** (Web) — concrete `IClock` / `ILog` implementations wired up in `Program.cs`.

## Functional style with Funcky `Option<T>`

All "find" methods on repositories return `Option<T>`, not `T?`. **Never escape the monadic space** by extracting to a nullable. Stay inside `Option<T>` using the tools below.

### Creating Option values

```csharp
Option.Some(value)          // wrap a known value
Option<T>.None              // empty
dict.GetValueOrNone("key")  // Funcky extension — replaces TryGetValue
sequence.FirstOrNone(pred)  // Funcky LINQ extension — replaces FirstOrDefault
```

### List pattern matching — the idiomatic `if` for Option

`Option<T>` implements the list pattern, so use C# pattern matching instead of `TryGetValue` or `.Match(none: () => default!, ...)`:

```csharp
// Guard: act only when Some, skip when None
if (option is [var value])
{
    // use value
}

// Early return when None
if (option is not [var value])
{
    return Response.NotFound();
}
// use value

// Both branches
if (option is [var value])
    DoSomething(value);
else
    HandleMissing();
```

### Staying in monadic space

```csharp
option.Select(x => x.Name)            // map: Option<T> → Option<U>
option.SelectMany(x => Find(x.Id))    // flatMap / bind: Option<T> → Option<U>
option.GetOrElse("fallback")          // extract with default (only at edges)
option.Match(none: ..., some: ...)    // bifurcate (prefer list pattern for guards)
option.ToEnumerable()                 // Option<T> → IEnumerable<T> (0 or 1 items)
```

### Async pattern

When the `some` branch needs an async call, use `Match` with `Task`-returning lambdas:

```csharp
var userOption = await emailOption.Match(
    none: () => Task.FromResult(Option<User>.None),
    some: async email => await env.Users.FindByEmail(email));
```

### Pipeline example from the codebase

```csharp
// AuthHandlers.Register — stays entirely in Option space
if (decoded.Email is [var regEmail])
{
    if ((await env.Users.FindByEmail(regEmail)) is not [var existingUser])
    {
        // user not found — create one
        var roleNames = (await env.Roles.FindByName("Benutzer"))
            .Select(role => role.Name)
            .ToEnumerable()
            .ToImmutableList();
        existingUser = User.Create(...);
        await env.Users.Save(existingUser);
    }
    // existingUser is guaranteed non-null here
    ...
}
```

### What NOT to do

```csharp
// BAD — escapes monadic space, brings nullability back in
var x = option.Match(none: () => default(T), some: v => v);
if (x is null) return Response.NotFound();

// BAD — TryGetValue is banned by the Funcky analyzer (λ0001)
if (!option.TryGetValue(out var x)) return Response.NotFound();

// GOOD — list pattern keeps everything in Option
if (option is not [var x]) return Response.NotFound();
// use x here
```

### UI language

All user-facing text (form validation messages, article content, page labels) is in **German**.

### File layout

One public type per file; no namespaces (relies on the implicit global namespace and `ImplicitUsings`).

**`FunctionalBlog.Domain/`**: `Article.cs`, `ArticleId.cs`, `ArticleTitle.cs`, `ArticleText.cs`

**`FunctionalBlog.Application/`**: `IArticleRepository.cs`

**`FunctionalBlog.DataAccess.InMemory/`**: `InMemory*` repository implementations grouped by domain area (`Articles/`, `Identity/`, `Recipes/`, `Roles/`, `Translations/`).

**`FunctionalBlog.DataAccess.Sqlite/`**: `Sqlite*` repository implementations, `DatabaseMigrator`, `DapperTypeHandlers`, `SqliteConnectionFactory`, `Pbkdf2PasswordHasher` (`Identity/`), `TranslationSeeder` (`Translations/`), and `Migrations/0001_initial_schema.sql` (embedded resource).

**`FunctionalBlog/`** (Web), grouped by role:

1. Functional core: `Effect.cs`, `App.cs`, `Middleware.cs`, `Functional.cs`
2. HTTP model: `Request.cs`, `Response.cs`, `Env.cs`
3. Pipeline: `Middlewares.cs`, `Router.cs`
4. Application: `BlogHandlers.cs`, `StaticHandlers.cs`
5. Form: `ArticleForm.cs`, `DecodedArticleForm.cs`
6. Views: `BlogViews.cs`, `Layout.cs`, `Html.cs`, `wwwroot/styles.css`
7. Infrastructure: `IClock.cs`, `SystemClock.cs`, `ILog.cs`, `ConsoleLog.cs`, `HttpAdapter.cs`
8. Bootstrap: `Program.cs`
