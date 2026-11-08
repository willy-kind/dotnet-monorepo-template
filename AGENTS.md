# AGENTS.md

Instructions for AI coding agents working in this repo.

## How to work

The best code is the code never written. Lazy means writing less code, never skipping a rule, a test or the definition of done. If anything in this section conflicts with the rest of the file, the rest of the file wins.

First read the task, the code it touches and the real flow through it. Then:

1. Does it need to be built at all?
2. Does the repo already have it? Check neighbouring slices, `Common`, `Domain` and `Integrations`.
3. Does the BCL, ASP.NET Core or the .NET host cover it?
4. Does a package already referenced cover it?
5. Post a short plan and wait for approval: what you will reuse, what you will change, which files, which tests, and any breaking change. A few bullets, no more.
6. Only then, write the minimum code that works, following the existing pattern.

A breaking change is anything that can break a caller, an external system or a deployment: removing or renaming an endpoint, a field or a config key; changing a status code, a type or what is required; changing what a job sends or how often. Never make one without approval, even mid-task.

- No abstractions, options or scaffolding nobody asked for. Prefer deleting over adding, and boring over clever.
- Never cut validation, error handling, security or real edge cases to keep code short.
- Bug fixes: fix the root cause, with a test that fails first. Find every caller. If the bug is duplicated in several slices, fix each copy.
- Big requests: plan the smallest version that does the job and list what you would leave out.

## Repo

Monorepo of .NET APIs (`WebApplication.CreateBuilder`) and workers (`Host.CreateApplicationBuilder`). No database.

```
src/<App>/
  Features/<Area>/<VerbNoun>.cs   # slices (+ optional <VerbNoun>.Models.cs)
  Common/                         # shared helpers
  Domain/                         # shared business concepts and rules
  Integrations/                   # thin clients for external systems
tests/<App>.Tests/                # mirrors src/<App>
```

While iterating, run `dotnet format`, `dotnet build` and `dotnet test` scoped to your app's project.

`ArchitectureTests` and `BannedSymbols.txt` enforce the slice and integration rules below. When one fails, fix the code. Never change, skip or weaken a check, its config or its test, and never suppress a warning (`#pragma`, `SuppressMessage`, `NoWarn`). The one exception is the existing `NoWarn` for CS1591 in `Directory.Build.props`: XML docs are not required.

Don't change `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig` or `BannedSymbols.txt` unless asked. Adding a package version is the one exception, and only after the package is approved.

## Slices

`Features/` holds slices, grouped by area (`Features/Orders/`). A slice is one endpoint or one job: a `public static partial class` named after it, with everything only it needs nested inside, registered explicitly in `Program.cs`. It has at most two files, side by side: `<Slice>.cs` for the logic, and `<Slice>.Models.cs` for its request, response and DTOs when separating them helps.

**Slices never reference each other, and duplication between them is fine.** Coupling costs more than repeated lines. Promote code only when a second slice actually needs it. Move it, don't copy it:

| What it is | Where it goes |
|---|---|
| Business concept, rule, entity, value object | `Domain/` |
| Talking to an external system | `Integrations/` |
| Anything else | `Common/` |

**Integrations are thin.** A typed client, DTOs that match the external API as-is, options, auth (with token caching) and an `Add<Name>Client` registration. Return `null` for statuses the API uses meaningfully (e.g. 404), `EnsureSuccessStatusCode()` otherwise. No logic, mapping, data caching, orchestration or retries, and no wrapper interfaces.

## APIs

New endpoint: follow `src/Example.Api/Features/Orders/CreateOrder.cs`.

| Situation | Response |
|---|---|
| Invalid request | 400 `ValidationProblem` |
| Not found | 404 |
| Conflict | 409 |
| External system failed or timed out | 502 `Problem`, title `"<System> is unavailable."` |
| Anything else | Don't catch it (500) |

Around integration calls, catch only `HttpRequestException` and `TaskCanceledException when (!ct.IsCancellationRequested)`. Validate the way the app already does; if it has no approach, use a private `Validate` method like the example.

## Jobs

New job: follow `src/Example.Job/Features/Customers/SyncCustomers.cs`.

`RunOnceAsync` does one run and lets integration failures propagate. `ExecuteAsync` only loops: call `RunOnceAsync`, log any exception, wait for the next interval. That loop is the only place `catch (Exception)` is allowed. The next interval is the retry.

## Tests

New test: follow `tests/Example.Api.Tests/Features/Orders/CreateOrderTests.cs` or `tests/Example.Job.Tests/Features/Customers/SyncCustomersTests.cs`.

Test slices through their real entry point: HTTP for APIs, `RunOnceAsync` for jobs, both via `TestApp`. Never test a handler directly or with mocks. Per slice:

1. **One happy path.** For jobs, assert what was sent (`FakeHttpHandler.Requests`).
2. **One test per deliberate failure outcome**, not per input. APIs: 400, 404, 409, 502 (an upstream 500 and a timeout are one test). Jobs: integration failure, plus each case handled on purpose.
3. **A `[Theory]`** for pure logic with many cases: an `internal static` method tested in the slice's test file, or under `Domain/`/`Common/` once promoted.

Usually 2 to 4 tests; more than 6 means logic should move into a `[Theory]`. Don't test framework behaviour, DI, DTOs, one-line mappings or logging.

Tests must pass with no network. Fake only the outside world: `FakeHttpHandler` (`Respond`, `Throw`, `Requests`) for HTTP, hand-written fakes in `TestApp` for anything else. A new integration needs one `FakeHttpHandler` property in `TestApp` and, in a job's `TestApp`, its `Add<Name>Client` call and each new job.

## Done

- [ ] `dotnet format`, `dotnet build` and `dotnet test` pass from the repo root with zero warnings
- [ ] Each slice you touched has its tests
- [ ] Summary: what changed, what you left out, what to verify by hand, and any rule you disagreed with

## Stop and ask when

- A new package, project or app seems needed
- The change turns out to be breaking and wasn't in the approved plan
- A check or build rule seems wrong
- One slice would need to call another
- The task needs a database
- A client's retry setup gets in the way of tests
- The requirements are ambiguous in a way that changes the design
