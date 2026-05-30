# Release Workflow

This document describes the process for creating builds and releases for the Playlist application.

## Overview

The project uses a dual-mode release script (`release.ps1`) that supports both:
- **Development builds** - Local testing with timestamped installers
- **Release builds** - Versioned releases with git tags and GitHub integration

## Git Workflow

### Branch Strategy

- **`main`** - Production-ready code, releases only
- **`develop`** - Integration branch for testing and pre-release validation
- **Feature branches** - All development work (e.g., `feature/style-changes`)

### Development Process

1. Create a feature branch for your work:
   ```powershell
   git checkout -b feature/your-feature-name
   ```

2. Make changes and commit regularly:
   ```powershell
   git add .
   git commit -m "Your commit message"
   ```

3. When ready, merge to main:
   ```powershell
   git checkout main
   git merge feature/your-feature-name
   ```

## Creating Builds

### Development Builds (Testing)

Use this for local testing without creating a release.

Branch rules:
- Run test builds from `develop` or a working branch.
- `main` is production-only and will reject non-forward or test-style version runs.
- Non-main branches never create tags, never push, and never publish to GitHub.
- Non-main test builds skip release-note generation entirely.

1. Run the release script:
   ```powershell
   .\release.ps1
   ```

2. The script first announces that this is a **test/dev release build** and that it will skip release-note generation and publishing.

3. When prompted for the **test/develop version**, either:
   - **Press Enter** to keep the current version, or
   - **Enter a lower or alternate test version** such as `1.2.1`

4. The script will:
   - Temporarily update `src/Playlist/Playlist.csproj` if needed for the test build
   - Build the application
   - Create a timestamped installer: `PlaylistSetup_YYYY-MM-DD-HH-mm.exe`
   - Revert temporary changes to `src/Playlist/Playlist.csproj` and `setup.iss`
   - Skip `RELEASE_NOTES.md`, rc_ diff generation, tagging, and publishing
   - **No git commit, tag, or push**

5. Find your installer in the `installer/` directory

6. After the build finishes, the working tree returns to the original production version values unless you make additional manual edits yourself

### Release Builds (Production)

Use this when ready to publish a new version.

1. **Ensure you're on the main branch**:
   ```powershell
   git checkout main
   ```

2. Run the release script:
   ```powershell
   .\release.ps1
   ```

3. **Enter the new version** when prompted (e.g., `1.0.0`, `1.1.0`)

   Production rule:
   - The version entered on `main` must be greater than the current project version.
   - If it is equal to or less than the current version, the script stops and instructs you to use `develop` or a working branch for test builds.

4. The script will:
   - Update version in `src/Playlist/Playlist.csproj`
   - Generate a diff of all changes since the last release
   - Create a release notes file: `rc_<version>_<timestamp>.txt`
   - Open the file for you to review and edit

5. **Edit the release notes file** to describe the changes

6. Return to the terminal and confirm with `y` when prompted "Continue? y/[n]"

7. The script will:
   - Build the application
   - Create the installer: `PlaylistSetup.exe`
   - Copy release notes to `RELEASE_NOTES.txt` for GitHub Actions
   - Commit all changes with message: "Bump version to X.X.X"
   - Create a git tag: `vX.X.X`
   - Push the commit and tag to GitHub

8. **GitHub Actions takes over**:
   - Automatically triggered by the version tag
   - Builds the application
   - Creates the installer
   - Reads `RELEASE_NOTES.txt` from the repository
   - Creates a GitHub Release with the installer and your custom release notes

## Release Script Details

### Prerequisites

- PowerShell
- .NET SDK 9.0 or later
- Inno Setup installed at: `C:\Program Files (x86)\Inno Setup 6\ISCC.exe`
- Git configured with remote repository

### Key Features

- **Branch enforcement** - `main` is production-only; non-main branches are local-test-only
- **Version validation** - Ensures semantic versioning format
- **Version safety on test builds** - Temporarily applies test versions on non-main branches and reverts them after the build
- **Automatic diff generation** - Shows all changes since last tag
- **RC file preservation** - Release candidate files kept for reference
- **Revert on cancel** - Safely backs out changes if you cancel
- **Dev build cleanup** - Reverts `src/Playlist/Playlist.csproj` and `setup.iss` after development builds

### Release Notes Files

The script creates `rc_<version>_<timestamp>.txt` files containing:
- Git diff output from the last tag to current HEAD
- All file changes and line-by-line differences

The timestamped files are preserved for historical reference. When you confirm the release, the script:
1. Copies the edited rc_ file to `RELEASE_NOTES.txt` (fixed filename)
2. Commits both files to the repository
3. GitHub Actions uses `RELEASE_NOTES.txt` as the release body on GitHub

**Best Practice:** Edit the rc_ file to create user-friendly release notes before confirming the build. Remove the raw diff output and replace it with a clean summary of changes for users.

## GitHub Actions Workflow

The `.github/workflows/release.yml` workflow:
- Triggers on tags matching `v*.*.*` pattern
- Builds a self-contained application with Windows x64 runtime
- Compiles the Inno Setup installer
- Reads `RELEASE_NOTES.txt` from the repository for the release body
- Creates a GitHub Release with the installer and custom release notes
- Creates a GitHub Release with the installer attached

## Versioning

This project follows [Semantic Versioning](https://semver.org/):

- **MAJOR.MINOR.PATCH** (e.g., 1.0.0)
  - **MAJOR** - Breaking changes
  - **MINOR** - New features (backward compatible)
  - **PATCH** - Bug fixes

Version is stored in three places in `src/Playlist/Playlist.csproj`:
- `<Version>`
- `<AssemblyVersion>`
- `<FileVersion>`

The release script updates all three automatically.

## Troubleshooting

### Script won't run
- Ensure PowerShell execution policy allows scripts:
  ```powershell
  Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
  ```

### Inno Setup not found
- Install from: https://jrsoftware.org/isinfo.php
- Or update the path in `release.ps1`

### GitHub Actions not triggering
- Ensure tag format is `v*.*.*` (e.g., `v1.0.0`)
- Check that tag was pushed: `git push --tags`

### Test build used the wrong branch
- Test builds should be created from `develop` or a feature branch
- `main` is reserved for production releases only
- If you need to test update flows against a lower version, switch to a non-main branch first

### Build fails
- Ensure all changes are committed before running release
- Check .NET SDK version: `dotnet --version`
- Verify project builds manually: `dotnet build src/Playlist/Playlist.csproj`

## Quick Reference

| Task | Command |
|------|---------|
| Create feature branch | `git checkout -b feature/name` |
| Test build | `.\release.ps1` on `develop`/feature branch |
| Release build | `.\release.ps1` → **enter new version** |
| Check current branch | `git branch` |
| Switch to main | `git checkout main` |
| Switch to develop | `git checkout develop` |
| View last tag | `git describe --tags --abbrev=0` |
| View all tags | `git tag -l` |

## Additional Resources

- [Inno Setup Documentation](https://jrsoftware.org/ishelp/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Semantic Versioning](https://semver.org/)
