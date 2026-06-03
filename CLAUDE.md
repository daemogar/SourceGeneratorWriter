# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Purpose

This is a **reusable helper library for Roslyn source generators**. Its job is to provide the
indented-text-writer "block" helpers that make emitting nicely-formatted C# source easy, so that
multiple generator projects can share them instead of each copying their own.

The helpers originated in **SQuiL** (`../SQuiL`, a SQL-to-C# source generator) and are being
extracted here to be reusable. Note the dependency direction:

- This project will be **consumed by** SQuiL and other generator projects.
- This project does **not** depend on or use SQuiL itself — keep it generator-agnostic. Nothing
  SQuiL-specific (SQL parsing, data-context models, `[SQuiLQuery]`, etc.) belongs here.

## What's being extracted

Two files from `../SQuiL/SQuiL.SourceGenerator/System/CodeDom.Compiler/`:

- **`IndentedTextWriterBlock`** — a discriminated-union type holding *either* a string header
  fragment *or* an `Action` callback, with implicit conversions both ways so callers pass either
  without wrapping. Used as the `params` element type for `Block`.
- **`IndentedTextWriterExtensions.Block(...)`** — an extension method on the BCL
  `System.CodeDom.Compiler.IndentedTextWriter`. It writes a header line (honoring leading-tab
  indentation in the string), optionally invokes a callback wrapped in `{` / `}` with an increased
  indent level, then repeats for any additional `IndentedTextWriterBlock` params (alternating
  headers and braced callbacks — e.g. `try { } catch { }`).

### Key conventions to preserve

- Keep these types in namespace **`System.CodeDom.Compiler`**. That's deliberate: it makes the
  `Block` extension and the `IndentedTextWriterBlock` conversions available transparently wherever
  the BCL `IndentedTextWriter` is already in scope, with no extra `using`.
- The `Block` header logic splits on the writer's `NewLine`, counts leading `\t` characters per
  line, and adjusts `writer.Indent` accordingly — so raw-string-literal headers with tab indentation
  format correctly. Preserve this behavior; SQuiL's generated output depends on it.

## Project / build conventions

Match SQuiL's generator project so consumers can reference this from an analyzer build:

- Target **`netstandard2.0`** (the analyzer/source-generator runtime requirement).
- `ImplicitUsings`, `Nullable`, `LangVersion=latest`.
- Set `EnforceExtendedAnalyzerRules=true` — analyzer-referenced libraries must obey these rules, so
  validating here prevents breakage in consuming generators.

```powershell
# Add a project to the (.slnx XML-format) solution
dotnet new classlib -n SourceGeneratorWriter
dotnet sln SourceGeneratorWriter.slnx add SourceGeneratorWriter/SourceGeneratorWriter.csproj

dotnet build
dotnet test
dotnet test --filter "FullyQualifiedName~SomeTestClass.SomeTest"   # single test
```

## Release & publishing

CI mirrors SQuiL's pipeline (`.github/workflows/`), trimmed to this repo's single NuGet package:

- **`build.yml`** — on push/PR to `master`: restore, build Release, run tests (CI on every PR commit).
- **`publish.yml`** — builds, computes a version, creates a GitHub release, packs, and
  `dotnet nuget push`es to nuget.org. It fires on **push to `master`** and on a
  **`pull_request_review` that approves** a PR (`if: github.event.review.state == 'approved'`).
  The approval path checks out the PR's head commit (`github.event.pull_request.head.sha`).

Versioning matches SQuiL: `VERSION = <dotnet --version>.<run_number padded to 4>-beta`
(e.g. `10.0.300.0001-beta`), tagged as a GitHub **prerelease**. **Both** triggers currently produce a
`-beta` prerelease — push-to-master and approved-PR alike.

**Official (stable) releases are not wired up yet** (intentional). The plan is to drive them from an
explicit trigger — most likely a version-tag push (`on: push: tags: ['v*']`) — with `-beta` dropped
and `prerelease: false`. See the TODO comment at the top of `publish.yml` before adding it.

Required GitHub secrets (Settings → Secrets and variables → Actions):

- `NUGET_API_KEY` — a nuget.org API key scoped to push `SourceGeneratorWriter` (the package name
  must be reserved/owned by that account, or use a glob-scoped key for first publish).
- `GITHUB_TOKEN` is provided automatically; no setup needed.

`PackageProjectUrl`/`RepositoryUrl` and the release-notes link point at
`https://github.com/daemogar/SourceGeneratorWriter`. Manual local pack:

```powershell
dotnet pack SourceGeneratorWriter/SourceGeneratorWriter.csproj -c Release `
  --include-symbols -p:SymbolPackageFormat=snupkg -p:Version=1.0.0
```

## Source-generator skill

The `squil` skill covers the SQuiL generator that will consume this library. It's relevant for
understanding the downstream consumer, but remember this project stays SQuiL-agnostic.
