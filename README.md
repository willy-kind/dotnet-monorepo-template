# mono-dotnet-repo-template

Template for .NET mono repos, built on the `examplerepo` setup (direnv + nix flake).

## Setup

1. Copy this directory and rename it.
2. Replace `MonoDotnet` in `.dev/flake.nix` and rename `MonoRepo.slnx`.
3. Rename or replace the `Example.*` projects.
4. `git init && git add -A` (flakes only see files tracked by git).
5. `direnv allow`

The dev shell provides the .NET 10 SDK (`dotnet-sdk_10` from nixpkgs), sops and age,
and enables the conventional-commits git hook.

## Layout

```
.dev/                     flake.nix dev shell, githooks
src/
  Example.Api/            minimal web API
  Example.Job/            background job (generic host)
tests/
  Example.Api.Tests/      xUnit v3 tests for Example.Api
  Example.Job.Tests/      xUnit v3 tests for Example.Job
  Shared/                 FakeHttpHandler and ArchitectureTests, linked into every test project
Directory.Build.props     shared MSBuild settings (TFM, nullable, analyzers, warnings as errors)
Directory.Packages.props  central package versions
global.json               SDK version pin
MonoRepo.slnx             solution
```

## Common commands

```sh
dotnet build                                   # build everything
dotnet test                                    # run all tests
dotnet run --project src/Example.Api           # run the API
dotnet run --project src/Example.Job           # run the job
```

## Adding a project

```sh
dotnet new web     -n My.Api -o src/My.Api                 # an API
dotnet new console -n My.Job -o src/My.Job                 # a job (also reference Microsoft.Extensions.Hosting)
dotnet new console -n My.Api.Tests -o tests/My.Api.Tests   # its test project
dotnet sln add src/My.Api tests/My.Api.Tests
dotnet add tests/My.Api.Tests reference src/My.Api
```

Then remove `TargetFramework`, `Nullable`, `ImplicitUsings` and package `Version`
attributes from the new csproj files and delete the test project's `Program.cs`:
they come from `Directory.Build.props` and `Directory.Packages.props`. Projects
ending in `.Tests` automatically become xUnit v3 test projects and get the shared
`FakeHttpHandler` and `ArchitectureTests`. An API's test project also references
`Microsoft.AspNetCore.Mvc.Testing`.

## Commits

Commits must follow [Conventional Commits](https://www.conventionalcommits.org/),
e.g. `feat(api): add hello endpoint`. Enforced by `.dev/githooks/commit-msg`
locally, and by CI for every commit in a pull request.

## CI

`.github/workflows/ci.yml` runs on pushes to `main`, pull requests and weekly:
`dotnet format --verify-no-changes`, `dotnet build`, a vulnerable-package check,
and `dotnet test` with coverage (Cobertura, uploaded as the `coverage` artifact).
