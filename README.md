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
  Example.Core/           class library
  Example.Api/            minimal web API (references Core)
tests/
  Example.Core.Tests/     xunit tests for Core
Directory.Build.props     shared MSBuild settings (TFM, nullable, analyzers, warnings as errors)
Directory.Packages.props  central package versions
global.json               SDK version pin
MonoRepo.slnx             solution
```

## Common commands

```sh
dotnet build                                   # build everything
dotnet test                                    # run all tests
dotnet run --project src/Example.Api           # run the API, try GET /hello/{name}
```

## Adding a project

```sh
dotnet new classlib -n My.Lib -o src/My.Lib
dotnet new xunit    -n My.Lib.Tests -o tests/My.Lib.Tests
dotnet sln add src/My.Lib tests/My.Lib.Tests
dotnet add tests/My.Lib.Tests reference src/My.Lib
```

Then remove `TargetFramework`, `Nullable`, `ImplicitUsings` and package `Version`
attributes from the new csproj files: they come from `Directory.Build.props` and
`Directory.Packages.props`. Projects ending in `.Tests` are marked as test projects
automatically.

## Commits

Commits must follow [Conventional Commits](https://www.conventionalcommits.org/),
e.g. `feat(api): add hello endpoint`. Enforced by `.dev/githooks/commit-msg`.
