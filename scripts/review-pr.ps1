<#
.SYNOPSIS
    Fetches the PR diff for the current branch, sends it to Claude CLI for
    review, and saves the review locally. Use -AutoPost to also post it to GitHub.

.PARAMETER AutoPost
    Post the review to GitHub as a PR comment. Without this flag, the review
    is only saved locally. Alias: -y

.EXAMPLE
    .\scripts\review-pr.ps1
    .\scripts\review-pr.ps1 -AutoPost
    .\scripts\review-pr.ps1 -y
#>
param(
    [Alias("y")]
    [switch]$AutoPost
)

$ErrorActionPreference = "Stop"
$repo = "jibedoubleve/lanceur-bis"

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# ── 1. Resolve PR number from current branch ────────────────────────────────
Write-Host "Detecting PR for current branch..." -ForegroundColor Cyan
$prJson = gh pr view --json number 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "No PR found for the current branch. Make sure a PR exists for this branch."
    exit 1
}
$PrNumber = ($prJson | ConvertFrom-Json).number
Write-Host "Detected PR #$PrNumber" -ForegroundColor Green

# ── 2. Fetch PR metadata ────────────────────────────────────────────────────
Write-Host "Fetching PR #$PrNumber metadata..." -ForegroundColor Cyan
$metaJson = gh pr view $PrNumber --json title,url,headRefName,baseRefName,files 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to fetch PR #$PrNumber metadata."
    exit 1
}
$meta = $metaJson | ConvertFrom-Json
$prTitle = $meta.title
$prUrl   = $meta.url
$headRef = $meta.headRefName
$baseRef = $meta.baseRefName
$fileCount = $meta.files.Count

Write-Host "  Title : $prTitle" -ForegroundColor White
Write-Host "  Branch: $headRef -> $baseRef" -ForegroundColor White
Write-Host "  Files : $fileCount changed" -ForegroundColor White
Write-Host "  URL   : $prUrl" -ForegroundColor White

# ── 3. Fetch diff ────────────────────────────────────────────────────────────
Write-Host "Fetching diff..." -ForegroundColor Cyan
$diffFile = [System.IO.Path]::GetTempFileName() + ".diff"
try {
    $diffContent = gh pr diff $PrNumber 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to fetch diff for PR #$PrNumber."
        exit 1
    }
    $diffContent | Out-File -FilePath $diffFile -Encoding utf8
    $diffSize = (Get-Item $diffFile).Length
    Write-Host "  Diff saved to temp file ($([math]::Round($diffSize / 1024, 1)) KB)" -ForegroundColor White

    # ── 4. Call Claude CLI ───────────────────────────────────────────────────
    $reviewPrompt = @"
You are reviewing a GitHub Pull Request. Produce a structured markdown review.

**Repository:** $repo
**PR #${PrNumber}:** $prTitle
**URL:** $prUrl
**Branch:** ``$headRef`` -> ``$baseRef``

## Instructions

Analyse the diff provided and write a review with these sections:

### Overview
A brief summary (2-4 sentences) of what this PR does.

### Issues
List any bugs, logic errors, security concerns, or significant code-quality problems.
For each issue, include a direct link to the file on GitHub using this format:
[$repo/$filePath (line N)]($prUrl/files#diff-<use the file path>)
If there are no issues, write "No issues found."

### Suggestions
Minor improvements, nitpicks, naming, style, or small refactors.
If none, write "No suggestions."

### What Looks Good
Highlight positive aspects of the PR (good patterns, clean code, nice tests, etc.).

Keep the review concise and actionable.
"@

    Write-Host "Sending diff to Claude for review..." -ForegroundColor Cyan
    $reviewOutput = Get-Content $diffFile -Raw | claude --print "$reviewPrompt"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Claude CLI failed. Make sure 'claude' is installed and configured."
        exit 1
    }

    # ── 5. Save locally ─────────────────────────────────────────────────────
    $reviewDir = Join-Path $PSScriptRoot ".." "_review"
    if (-not (Test-Path $reviewDir)) {
        New-Item -ItemType Directory -Path $reviewDir -Force | Out-Null
    }
    $reviewFile = Join-Path $reviewDir "pr-$PrNumber-review.md"
    $reviewOutput | Out-File -FilePath $reviewFile -Encoding utf8
    Write-Host "Review saved to: $reviewFile" -ForegroundColor Green

    # ── 6. Post to GitHub (only with -AutoPost) ──────────────────────────────
    if ($AutoPost) {
        Write-Host "Posting review to PR #$PrNumber..." -ForegroundColor Cyan
        gh pr review $PrNumber --comment --body-file $reviewFile
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to post review to GitHub."
            exit 1
        }
        Write-Host "Review posted successfully!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "Review saved. To post it to GitHub, run:" -ForegroundColor Yellow
        Write-Host "  gh pr review $PrNumber --comment --body-file `"$reviewFile`"" -ForegroundColor White
    }

}
finally {
    # ── Cleanup ──────────────────────────────────────────────────────────────
    Remove-Item $diffFile -ErrorAction SilentlyContinue
}
