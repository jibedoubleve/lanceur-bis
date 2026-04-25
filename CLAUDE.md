# CLAUDE.md — Lanceur Bis

## Environment constraint

This project is **Windows-only** (WPF, Win32, `net9.0-windows10.0.19041.0`). Claude Code may be running on macOS where shell commands, builds, and tests **cannot execute locally**. In that case:

- Do not attempt `dotnet build`, `dotnet test`, or `dotnet cake` — they will fail.
- Read, edit, search, and analyze code freely; all file-based tools work normally.
- When a build or test run is needed, ask the user to run it on their Windows machine, or rely on the CI pipeline (GitHub Actions on `windows-latest`).

---

## Project overview

**Lanceur Bis** is a Windows application launcher (inspired by SlickRun/Flow Launcher) built with:

- **.NET 9 / C# / WPF** — UI framework
- **SQLite + Dapper** — persistence layer
- **NLua + NCalcSync** — Lua scripting and expression evaluation
- **Serilog** — structured logging
- **CommunityToolkit.Mvvm** — MVVM pattern
- **xUnit v3 + NSubstitute + Shouldly** — testing

---

## Architecture

```
src/
├── Lanceur.Core/            # Domain models, interfaces, business logic
├── Lanceur.SharedKernel/    # Cross-cutting concerns (logging, caching, IoC helpers)
├── Infra/
│   ├── Lanceur.Infra/          # General infrastructure (search, Lua, calculations)
│   ├── Lanceur.Infra.SQLite/   # SQLite repositories and migrations
│   └── Lanceur.Infra.Win32/    # Windows API integration
├── Libraries/
│   ├── Everything.Wrapper/     # Windows Everything search integration
│   ├── System.SQLite.Updater/  # DB schema updater
│   └── System.Web.Bookmarks/   # Web bookmark utilities
├── UI/
│   ├── Lanceur.Ui.Core/        # ViewModels, UI services
│   └── Lanceur.Ui.WPF/         # XAML views, app entry point
└── Tests/
    ├── Lanceur.Tests/           # Main test project (xUnit)
    └── Lanceur.Tests.Tools/     # Shared test helpers
```

Dependency direction: `UI` → `Core` ← `Infra`. Core has no dependency on UI or Infra.

---

## Build & tooling

Build tool is **Cake** (`build.cake`). Common targets:

| Goal | Command |
|------|---------|
| Build + test | `dotnet cake` |
| Build only | `dotnet cake --target=build` |
| Test only | `dotnet cake --target=test` |
| Create installer + ZIP | `dotnet cake --target=bin` |
| Full release to GitHub | `dotnet cake --target=github` |

Local tools are pinned in `.config/dotnet-tools.json` (Cake 6.1.0, GitVersion 6.7.0, GitReleaseManager 0.20.0). Run `dotnet tool restore` before first use.

---

## Code style

Defined in `.editorconfig` and enforced by ReSharper analyzers:

- Prefer **expression-bodied members** where the body fits on one line.
- Interfaces must be prefixed with `I`.
- No more than **3 chained invocation arguments per line** — break at each argument.
- Nullable reference types are **enabled** — always handle nullability explicitly; do not suppress warnings with `!` unless truly necessary.
- Do not add comments unless the logic is non-obvious; self-documenting names are preferred.

---

## Testing conventions

- Framework: **xUnit v3** with `Shouldly` assertions and `NSubstitute` mocks.
- Test project targets the same Windows TFM as the main app.
- UI-thread tests use `[StaFact]` / `[StaTheory]` from `Xunit.StaFact`.
- Fake data uses `Bogus`.
- Integration tests hit a **real SQLite database** — do not mock it.
- Test class names follow `<TestedClass>Should` or `<TestedClass>Tests` convention.

---

## Versioning & releases

- **GitVersion** manages semantic versions (`GitVersion.yml`, next: 3.13.0).
- Master branch uses `beta` pre-release tag until a release branch is cut.
- CI on push to `master` or `release/*` runs the full `github` Cake target: build → test → zip → Inno Setup installer → GitHub release.
- Do not manually edit version numbers; GitVersion derives them from git tags and branch names.

---

## CI/CD

| Workflow | Trigger | What it does |
|----------|---------|--------------|
| `on_push.yml` | push to `master` / `release/*` | Full release pipeline |
| `on_pr.yml` | PR opened/updated | Build + test only |
| `manual_build_test.yml` | Manual dispatch | Build + test |

All CI runs on `windows-latest`.

---

## Key files to know

| File | Purpose |
|------|---------|
| `build.cake` | All build logic |
| `GitVersion.yml` | Versioning strategy |
| `setup.iss` | Inno Setup installer definition |
| `.editorconfig` | C# code style rules |
| `.config/dotnet-tools.json` | Pinned local tool versions |
| `src/Lanceur.Core/` | Start here when understanding domain logic |
| `src/UI/Lanceur.Ui.WPF/` | App entry point and XAML |
