# Backfill Script for Release Notes
# Generates release notes for historical releases that are missing them
# Usage: .\release_backfill.ps1 -PreviousTag v1.2.1 -NextTag v1.2.2

param(
    [Parameter(Mandatory=$true)]
    [string]$PreviousTag,
    
    [Parameter(Mandatory=$true)]
    [string]$NextTag
)

$ErrorActionPreference = "Stop"

# Colors
$InfoColor = "Cyan"
$SuccessColor = "Green"
$WarningColor = "Yellow"
$ErrorColor = "Red"

Write-Host "=== Release Notes Backfill Script ===" -ForegroundColor $InfoColor
Write-Host ""

# Validate tags exist
Write-Host "Validating tags..." -ForegroundColor $InfoColor

$previousTagExists = git rev-parse --verify "$PreviousTag" 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Tag '$PreviousTag' does not exist" -ForegroundColor $ErrorColor
    exit 1
}

$nextTagExists = git rev-parse --verify "$NextTag" 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Tag '$NextTag' does not exist" -ForegroundColor $ErrorColor
    exit 1
}

Write-Host "✓ Previous tag: $PreviousTag" -ForegroundColor $SuccessColor
Write-Host "✓ Next tag: $NextTag" -ForegroundColor $SuccessColor
Write-Host ""

# Extract version from tag (remove 'v' prefix)
$nextVersion = $NextTag -replace '^v', ''

# Generate diff file
$timestamp = Get-Date -Format "yyyy-MM-dd-HH-mm"
$diffFile = "rc_${nextVersion}_${timestamp}_backfill.txt"

Write-Host "Generating diff between $PreviousTag and $NextTag..." -ForegroundColor $InfoColor
git diff "$PreviousTag..$NextTag" | Out-File -FilePath $diffFile -Encoding utf8

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to generate diff" -ForegroundColor $ErrorColor
    exit 1
}

Write-Host "Diff saved to $diffFile" -ForegroundColor $SuccessColor
Write-Host ""

# Clear RELEASE_NOTES.md for fresh notes
Write-Host "Clearing RELEASE_NOTES.md..." -ForegroundColor $InfoColor
"" | Set-Content "RELEASE_NOTES.md" -Encoding utf8

# Generate AI prompt
$aiPrompt = @"
Please update RELEASE_NOTES.md for version $nextVersion using the content from $diffFile

Follow the style and format defined in RELEASE_NOTES_STYLE.md.

Focus on:
- User-facing changes and benefits
- New features and improvements
- Bug fixes and their impact
- Breaking changes (if any)

The raw git diff is in $diffFile - transform it into clear, user-friendly release notes.

File locations:
- Source diff: $diffFile
- Target file: RELEASE_NOTES.md
- Style guide: RELEASE_NOTES_STYLE.md
"@

# Copy prompt to clipboard
try {
    Set-Clipboard -Value $aiPrompt
    Write-Host "✓ AI prompt copied to clipboard!" -ForegroundColor $SuccessColor
    Write-Host "  Paste it into your AI tool to generate release notes." -ForegroundColor $InfoColor
}
catch {
    Write-Host "AI Prompt (copy manually if clipboard failed):" -ForegroundColor $WarningColor
    Write-Host $aiPrompt -ForegroundColor Gray
}

Write-Host ""
Write-Host "When AI has updated RELEASE_NOTES.md, press any key to continue..." -ForegroundColor $WarningColor
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Validate RELEASE_NOTES.md
Write-Host ""
if (-not (Test-Path "RELEASE_NOTES.md")) {
    Write-Host "Error: RELEASE_NOTES.md not found" -ForegroundColor $ErrorColor
    exit 1
}

$releaseNotes = Get-Content "RELEASE_NOTES.md" -Raw
if ([string]::IsNullOrWhiteSpace($releaseNotes)) {
    Write-Host "Error: RELEASE_NOTES.md is empty" -ForegroundColor $ErrorColor
    exit 1
}

# Copy to output file
$outputFile = "release_notes_${NextTag}.md"
Write-Host "Creating output file: $outputFile" -ForegroundColor $InfoColor

$releaseNotes | Set-Content $outputFile -Encoding utf8 -NoNewline

Write-Host ""
Write-Host "✓ Release notes backfill complete!" -ForegroundColor $SuccessColor
Write-Host ""
Write-Host "Output file: $outputFile" -ForegroundColor $SuccessColor
Write-Host "  → Copy the contents of this file into the GitHub Release for $NextTag" -ForegroundColor $InfoColor
Write-Host ""
Write-Host "Diff file: $diffFile (you can delete this later)" -ForegroundColor $InfoColor
Write-Host ""
