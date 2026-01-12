# Release Script for Playlist
# Handles version bumping, building, and releasing

$ErrorActionPreference = "Stop"

# Colors
$InfoColor = "Cyan"
$SuccessColor = "Green"
$WarningColor = "Yellow"
$ErrorColor = "Red"

function Get-CurrentVersion {
    $csprojPath = "src\Playlist\Playlist.csproj"
    $content = Get-Content $csprojPath -Raw
    if ($content -match '<Version>([\d\.]+)</Version>') {
        return $matches[1]
    }
    throw "Could not find version in $csprojPath"
}

function Set-ProjectVersion {
    param([string]$NewVersion)
    
    $csprojPath = "src\Playlist\Playlist.csproj"
    $content = Get-Content $csprojPath -Raw
    
    # Update all version fields
    $content = $content -replace '<Version>[\d\.]+</Version>', "<Version>$NewVersion</Version>"
    $content = $content -replace '<AssemblyVersion>[\d\.]+\.0</AssemblyVersion>', "<AssemblyVersion>$NewVersion.0</AssemblyVersion>"
    $content = $content -replace '<FileVersion>[\d\.]+\.0</FileVersion>', "<FileVersion>$NewVersion.0</FileVersion>"
    
    $content | Set-Content $csprojPath -NoNewline
    Write-Host "Updated version to $NewVersion in Playlist.csproj" -ForegroundColor $SuccessColor
}

function Build-LocalInstaller {
    param(
        [string]$Version,
        [bool]$IsDevBuild = $false
    )
    
    Write-Host "`nBuilding local installer for version $Version..." -ForegroundColor $InfoColor
    
    # Clean previous build
    if (Test-Path "publish") { Remove-Item "publish" -Recurse -Force }
    if (Test-Path "installer") { Remove-Item "installer" -Recurse -Force }
    
    # Publish
    Write-Host "Publishing application..." -ForegroundColor $InfoColor
    dotnet publish src\Playlist\Playlist.csproj --configuration Release --runtime win-x64 --self-contained --output .\publish /p:PublishSingleFile=false
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    
    # Create installer directory
    New-Item -ItemType Directory -Force -Path .\installer | Out-Null
    
    # Update setup.iss
    Write-Host "Updating InnoSetup configuration..." -ForegroundColor $InfoColor
    $setupContent = Get-Content setup.iss -Raw
    $setupContent = $setupContent -replace '#define MyAppVersion ".*"', "#define MyAppVersion `"$Version`""
    
    # Set installer filename based on build type
    if ($IsDevBuild) {
        $timestamp = Get-Date -Format "yyyy-MM-dd-HH-mm"
        $installerName = "PlaylistSetup_$timestamp"
    } else {
        $installerName = "PlaylistSetup"
    }
    $setupContent = $setupContent -replace 'OutputBaseFilename=.*', "OutputBaseFilename=$installerName"
    $setupContent | Set-Content setup.iss -NoNewline
    
    # Compile installer
    Write-Host "Compiling installer with InnoSetup..." -ForegroundColor $InfoColor
    $innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    if (-not (Test-Path $innoSetupPath)) {
        throw "InnoSetup not found at $innoSetupPath"
    }
    
    & $innoSetupPath setup.iss
    if ($LASTEXITCODE -ne 0) {
        throw "Installer compilation failed"
    }
    
    # Cleanup publish folder
    Write-Host "Cleaning up build artifacts..." -ForegroundColor $InfoColor
    if (Test-Path "publish") { Remove-Item "publish" -Recurse -Force }
    
    Write-Host "`nInstaller created successfully: installer\$installerName.exe" -ForegroundColor $SuccessColor
}

function Revert-VersionChange {
    param([string]$OriginalVersion)
    
    Write-Host "Reverting version changes..." -ForegroundColor $WarningColor
    git checkout -- src\Playlist\Playlist.csproj 2>$null
    Write-Host "Version reverted to $OriginalVersion" -ForegroundColor $WarningColor
}

# Main script
Write-Host "=== Playlist Release Script ===" -ForegroundColor $InfoColor
Write-Host ""

# Check current branch
$currentBranch = git rev-parse --abbrev-ref HEAD
$isMainBranch = $currentBranch -eq "main"

Write-Host "Current branch: $currentBranch" -ForegroundColor $(if ($isMainBranch) { $SuccessColor } else { $WarningColor })
if (-not $isMainBranch) {
    Write-Host "Note: Tags will only be created and pushed from the main branch" -ForegroundColor $WarningColor
}
Write-Host ""

# Get current version
$currentVersion = Get-CurrentVersion
Write-Host "Current version: $currentVersion" -ForegroundColor $InfoColor

# Prompt for new version
$newVersionInput = Read-Host "New version [$currentVersion]"
$newVersion = if ([string]::IsNullOrWhiteSpace($newVersionInput)) { $currentVersion } else { $newVersionInput }

# If version unchanged, build local installer and exit
if ($newVersion -eq $currentVersion) {
    $timestamp = Get-Date -Format "yyyy-MM-dd-HH-mm"
    $localVersion = "$currentVersion-$timestamp"
    Write-Host "`nBuilding local installer (no version change)..." -ForegroundColor $InfoColor
    
    try {
        Build-LocalInstaller -Version $localVersion -IsDevBuild $true
        Write-Host "`nLocal build complete!" -ForegroundColor $SuccessColor
        
        # Revert setup.iss changes
        Write-Host "Reverting setup.iss..." -ForegroundColor $InfoColor
        git checkout -- setup.iss 2>$null
    }
    catch {
        Write-Host "Build failed: $_" -ForegroundColor $ErrorColor
        git checkout -- setup.iss 2>$null
        exit 1
    }
    
    exit 0
}

# Validate new version format
if ($newVersion -notmatch '^\d+\.\d+\.\d+$') {
    Write-Host "Invalid version format. Use X.Y.Z (e.g., 1.0.1)" -ForegroundColor $ErrorColor
    exit 1
}

# Generate diff for release notes BEFORE making changes
$timestamp = Get-Date -Format "yyyy-MM-dd-HH-mm"
$diffFile = "rc_${newVersion}_${timestamp}.txt"
Write-Host "`nGenerating diff for release notes..." -ForegroundColor $InfoColor

# Get last tag, or use empty tree if no tags exist (first release)
$lastTag = git describe --tags --abbrev=0 2>&1 | Where-Object { $_ -is [string] }
if ($lastTag) {
    Write-Host "Comparing changes since $lastTag..." -ForegroundColor $InfoColor
    git diff "$lastTag..HEAD" | Out-File -FilePath $diffFile -Encoding utf8
} else {
    Write-Host "No previous tags found, showing all changes..." -ForegroundColor $InfoColor
    $emptyTree = git hash-object -t tree /dev/null
    git diff "$emptyTree..HEAD" | Out-File -FilePath $diffFile -Encoding utf8
}

Write-Host "`nDiff saved to $diffFile" -ForegroundColor $SuccessColor
Write-Host "Please review and update the file with release notes." -ForegroundColor $InfoColor
Write-Host "Press any key when ready to continue..." -ForegroundColor $WarningColor
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Prompt to continue or cancel
Write-Host ""
$choice = Read-Host "Continue with release? (y/[n])"

if ($choice -notmatch '^y(es)?$') {
    Write-Host "Release cancelled" -ForegroundColor $WarningColor
    # Remove the rc_ file since we're cancelling
    if (Test-Path $diffFile) { Remove-Item $diffFile }
    exit 0
}

# Validate release notes file
if (-not (Test-Path $diffFile)) {
    Write-Host "Release notes file not found: $diffFile" -ForegroundColor $ErrorColor
    exit 1
}

$releaseNotes = Get-Content $diffFile -Raw
if ([string]::IsNullOrWhiteSpace($releaseNotes)) {
    Write-Host "Release notes file is empty. Please add release notes." -ForegroundColor $ErrorColor
    exit 1
}

# Update version in csproj
try {
    Set-ProjectVersion -NewVersion $newVersion
}
catch {
    Write-Host "Failed to update version: $_" -ForegroundColor $ErrorColor
    exit 1
}

# Build installer
Write-Host "`nBuilding installer..." -ForegroundColor $InfoColor
try {
    Build-LocalInstaller -Version $newVersion -IsDevBuild $false
}
catch {
    Write-Host "Build failed: $_" -ForegroundColor $ErrorColor
    Write-Host "Reverting changes..." -ForegroundColor $WarningColor
    Revert-VersionChange -OriginalVersion $currentVersion
    exit 1
}

# Copy release notes to fixed filename for GitHub Actions
Write-Host "`nPreparing release notes for GitHub..." -ForegroundColor $InfoColor
Copy-Item $diffFile "RELEASE_NOTES.txt" -Force
Write-Host "Release notes copied to RELEASE_NOTES.txt" -ForegroundColor $SuccessColor

# Commit changes
Write-Host "`nCommitting changes..." -ForegroundColor $InfoColor
git add .
git commit -F $diffFile

if ($LASTEXITCODE -ne 0) {
    Write-Host "Commit failed" -ForegroundColor $ErrorColor
    exit 1
}

Write-Host "Changes committed successfully" -ForegroundColor $SuccessColor

# Create and push tag (only on main branch)
if ($isMainBranch) {
    $tag = "v$newVersion"
    Write-Host "`nCreating tag: $tag" -ForegroundColor $InfoColor
    git tag -a $tag -m "Release $newVersion"
    
    Write-Host "Pushing commit and tag to origin..." -ForegroundColor $InfoColor
    git push origin $currentBranch
    git push origin $tag
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Push failed" -ForegroundColor $ErrorColor
        exit 1
    }
    
    Write-Host "`nRelease complete! Tag $tag pushed to GitHub." -ForegroundColor $SuccessColor
    Write-Host "GitHub Actions will build and publish the release." -ForegroundColor $InfoColor
}
else {
    Write-Host "`nChanges committed locally (no tag created - not on main branch)" -ForegroundColor $SuccessColor
}

Write-Host "`nDone!" -ForegroundColor $SuccessColor
