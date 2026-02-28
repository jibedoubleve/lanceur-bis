<#
.SYNOPSIS
    Converts Markdown (.md) files to PDF using pandoc.

.DESCRIPTION
    Converts one or all Markdown files in the current directory to PDF
    using pandoc with the XeLaTeX engine. Optionally cleans up source
    and pre-existing PDF files.

.PARAMETER AutoClear
    When specified, removes existing PDF files before conversion and
    deletes the source .md files after successful conversion.

.PARAMETER Path
    Path to a specific .md file to convert. If omitted, all .md files
    in the current directory are converted.

.EXAMPLE
    .\convert.ps1
    Converts all .md files in the current directory to PDF.

.EXAMPLE
    .\convert.ps1 -AutoClear
    Removes existing PDFs, converts all .md files, then removes the source .md files.

.EXAMPLE
    .\convert.ps1 -file report.md
    Converts only report.md to PDF.
#>
param(
    [Alias("clear")]
    [switch]$AutoClear,
    [Alias("file")]
    $Path
)

$ErrorActionPreference = "Stop"

function private:Convert-File {
    param (
        [string]$Path,
        [switch]$AutoClear
    )

    if (-not (Test-Path $Path)) {
        Write-Host "File '$Path' does not exist." -ForegroundColor Red
        return
    }

    Write-Host "Converting $Path to HTML..." -NoNewline

    try {
        $outPath = [System.IO.Path]::ChangeExtension($Path, '.html')
        pandoc $Path -o $outPath --embed-resources --standalone 2>&1
        Write-Host " Done." -ForegroundColor Green

        if ($AutoClear) {
            Write-Host "Removing source file $Path..." -NoNewline
            Remove-Item $Path
            Write-Host " Done." -ForegroundColor Green
        }
    }
    catch {
        Write-Host " Failed." -ForegroundColor Red
        Write-Host "`t$_" -ForegroundColor Red
    }
}

function private:Convert-All {
    $files = Get-ChildItem  # List all files in the current directory

    # If -AutoClear is specified, remove any existing PDF files before starting conversion
    if ($AutoClear) {
        Write-Host "Clear all pdf files..." -NoNewline
        Remove-Item *.pdf
        Write-Host " Done." -ForegroundColor Green
    }

    try {
        foreach ($file in $files) {
            # Skip non-Markdown files
            if ($file.Extension -ne ".md") {
                continue
            }

            Write-Host "Converting $($file.FullName) to HTML..." -NoNewline
    
            # Convert the Markdown file to HTML using pandoc 
            $outPath = [System.IO.Path]::ChangeExtension($file.FullName, '.html')
            pandoc $file.FullName -o $outPath --embed-resources --standalone 2>&1
            Write-Host " Done." -foregroundcolor green
        }

        # If -AutoClear is specified, remove the source .md files after successful conversion
        if ($AutoClear) {
		Write-Host "Clear all md files..." -ForegroundColor Cyan -NoNewline
            Remove-Item *.md
            Write-Host " Done." -ForegroundColor Green
        }
    }
    catch {
        # Report which file failed and the error details
        Write-Host " Failed." -foregroundcolor red
        Write-Host "`t$_" -foregroundcolor red
    }
}

# Dispatch: convert a single file if -Path was provided, otherwise convert all
if ($PSBoundParameters.ContainsKey('Path')) {
    Convert-File -Path $Path -AutoClear:$AutoClear
}
else {
    Write-Host "No specific file provided. Converting all .md files in the current directory." -ForegroundColor Cyan
    Convert-All
}
