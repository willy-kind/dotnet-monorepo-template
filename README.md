# dotnet-monorepo-template

Template for .NET mono repos of APIs and background jobs, set up so AI coding agents
can build features safely. The rules agents follow are in [AGENTS.md](AGENTS.md)
(`CLAUDE.md` points there). The build and tests enforce them:

- `BannedSymbols.txt` bans assembly scanning, MediatR, AutoMapper, `new HttpClient()`
  and `DateTime.Now`-style clocks (use `TimeProvider`).
- `tests/Shared/ArchitectureTests.cs` checks that slices never reference each other,
  shared code never references features, every slice has a test class and every job
  has `RunOnceAsync`.
- `Directory.Build.props` and `.editorconfig` turn analyzer and style warnings into
  errors.

## Prerequisites

Either:

- **Nix (with flakes) and direnv**: the dev shell in `nix/` provides the .NET SDK and
  turns the commit hook on, or
- **a .NET 10 SDK** matching `global.json`, then run
  `git config core.hooksPath .githooks` once.

## Setup

1. Copy this directory and rename it.
2. Replace `MonoDotnet` in `nix/flake.nix` and rename `MonoRepo.slnx`.
3. Rename or replace the `Example.*` projects.
4. `git init && git add -A` (flakes only see files tracked by git).
5. With Nix: `direnv allow`.

## Layout

```
.githooks/                commit-msg hook, used locally and by CI
.github/workflows/        CI
nix/                      Nix dev shell (flake), for local development only
src/
  Example.Api/            minimal web API
  Example.Job/            background job (generic host)
tests/
  Example.Api.Tests/      xUnit v3 tests for Example.Api
  Example.Job.Tests/      xUnit v3 tests for Example.Job
  Shared/                 FakeHttpHandler and ArchitectureTests, linked into every test project
AGENTS.md                 rules for AI agents (CLAUDE.md points here)
BannedSymbols.txt         APIs the build rejects in source projects
Directory.Build.props     shared MSBuild settings (TFM, nullable, analyzers, warnings as errors)
Directory.Packages.props  central package versions
.editorconfig             code style, enforced in the build
global.json               SDK version pin
renovate.json             dependency updates (NuGet, GitHub Actions, Nix)
MonoRepo.slnx             solution
```

Each app is organised in vertical slices (details in [AGENTS.md](AGENTS.md)):

```
src/<App>/
  Features/<Area>/<VerbNoun>.cs   # one endpoint or one job (+ optional <VerbNoun>.Models.cs)
  Common/                         # shared helpers
  Domain/                         # shared business concepts and rules
  Integrations/                   # thin typed clients for external systems
tests/<App>.Tests/                # mirrors src/<App>
```

## Common commands

```sh
dotnet format                                  # fix formatting (CI runs --verify-no-changes)
dotnet build                                   # build everything, warnings are errors
dotnet test                                    # run all tests
dotnet run --project src/Example.Api           # run the API
dotnet run --project src/Example.Job           # run the job
```

## Configuration

Each integration reads its settings from a section named after it, e.g. `Erp:BaseAddress`.
`appsettings.json` holds placeholder addresses. To point an app at real systems locally,
use environment variables, which override `appsettings.json`:

```sh
Erp__BaseAddress=https://erp.internal/ dotnet run --project src/Example.Api
```

Tests never use real systems: `TestApp` routes every integration to a `FakeHttpHandler`.

## Adding a project

```sh
dotnet new web     -n My.Api -o src/My.Api                 # an API
dotnet new console -n My.Job -o src/My.Job                 # a job
dotnet new console -n My.Api.Tests -o tests/My.Api.Tests   # its test project
dotnet sln add src/My.Api tests/My.Api.Tests
dotnet add tests/My.Api.Tests reference src/My.Api
```

Then:

- Remove `TargetFramework`, `Nullable`, `ImplicitUsings` and package `Version`
  attributes from the new csproj files: they come from `Directory.Build.props` and
  `Directory.Packages.props`. Delete the test project's `Program.cs`.
- **API:** copy the problem-details setup from `src/Example.Api/Program.cs`
  (`AddProblemDetails`, `UseExceptionHandler`, `UseStatusCodePages`) in place of the
  template's hello endpoint.
- **Job:** match `src/Example.Job/Example.Job.csproj`: reference
  `Microsoft.Extensions.Hosting` and `Microsoft.Extensions.Http`, set
  `RunWorkingDirectory`, and copy `appsettings.json` to the output.
- **Tests:** projects ending in `.Tests` automatically become xUnit v3 test projects
  and get the shared `FakeHttpHandler` and `ArchitectureTests`. Add a `TestApp.cs`
  like the one in `tests/Example.Api.Tests` or `tests/Example.Job.Tests`. An API's
  test project also references `Microsoft.AspNetCore.Mvc.Testing`; a job's references
  `Microsoft.Extensions.TimeProvider.Testing`.

## Commits

Commits must follow [Conventional Commits](https://www.conventionalcommits.org/),
e.g. `feat(api): add order endpoint`. Enforced by `.githooks/commit-msg`
locally, and by CI for every commit in a pull request. The Nix dev shell turns the
hook on; without it, run `git config core.hooksPath .githooks` once.

## CI

`.github/workflows/ci.yml` runs on pushes to `main`, pull requests and weekly:
`dotnet format --verify-no-changes`, `dotnet build`, a vulnerable-package check,
and `dotnet test` with coverage (Cobertura, uploaded as the `coverage` artifact).
