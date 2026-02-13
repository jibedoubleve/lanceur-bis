

### Overview
This PR adds a PowerShell script (`review-pr.ps1`) that automates code review by fetching the diff for the current branch's PR, piping it to the Claude CLI for analysis, saving the review locally, and optionally posting it as a GitHub PR comment via the `-AutoPost` flag.

### Issues

1. **Temp file not created before use in `finally` block** — If the `gh pr diff` call on line 58 fails before `$diffFile` is written, the `finally` block (line 129) will attempt to `Remove-Item` on a path returned by `GetTempFileName()` that was already created as a 0-byte file by the OS, plus `.diff` was appended to a *new* string — meaning the original temp file (without `.diff`) is leaked and never cleaned up.
   [`review-pr.ps1` (line 55)](https://github.com/jibedoubleve/lanceur-bis/pull/1217/files)

2. **`$filePath` variable in the prompt template is undefined** — Line 82 references `$filePath` inside the here-string, but this variable is never assigned. The rendered prompt will contain an empty string where the file path placeholder should be.
   [`review-pr.ps1` (line 82)](https://github.com/jibedoubleve/lanceur-bis/pull/1217/files)

3. **`2>$null` silently swallows stderr on metadata fetch** — On line 37, stderr is discarded (`2>$null`), while line 26 captures it (`2>&1`). If the metadata call fails for an auth or network reason, the user gets a generic error with no diagnostics.
   [`review-pr.ps1` (line 37)](https://github.com/jibedoubleve/lanceur-bis/pull/1217/files)

### Suggestions

- **Hardcoded repo name** (line 21) — Consider deriving the repo from `gh repo view --json nameWithOwner` so the script is portable to forks or other repositories.
- **Review file encoding** — `[System.IO.File]::WriteAllText()` defaults to UTF-8 without BOM in .NET Core but UTF-8 *with* BOM in Windows PowerShell 5.1. If consistency matters, pass an explicit encoding parameter.
- **`Out-String` wrapping** (lines 63, 104) — Piping to `Out-String` can add a trailing newline and may not be necessary when `$diffContent` / `$reviewOutput` is already a string. Consider testing whether it's needed.

### What Looks Good

- Clean section-based structure with numbered steps and banner comments — easy to follow.
- Good use of `$ErrorActionPreference = "Stop"` combined with explicit `$LASTEXITCODE` checks for external tools.
- The `try/finally` pattern for temp-file cleanup is solid defensive coding.
- The `-AutoPost` / `-y` opt-in design is a safe default (no accidental posts).
- Helpful help block (`.SYNOPSIS`, `.EXAMPLE`) making the script self-documenting.
