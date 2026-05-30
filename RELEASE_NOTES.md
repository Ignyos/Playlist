# Release v1.2.9

## Overview
This release makes updates easier to discover and install from the main app screen. Playlist now checks for updates automatically and shows a clear in-app notice when a new version is available.

## New Features
- **Inline Update Notice**: Adds a green New Version Available! notice in the top menu area that appears only when an update exists.
- **One-Click Update Start**: Makes the inline notice clickable so you can begin the download and installer flow directly from the main window.

## Improvements
- **Automatic Update Checks**: Adds background update checks on app load and then every 24 hours based on last attempted check time.
- **Download Feedback**: Shows in-app status while the update download is in progress before launching the installer.
- **Manual + Automatic Check Coordination**: Keeps About > Check for Updates and background checks aligned so manual checks also reset update-check timing.

## Bug Fixes
- **Update Visibility Gap**: Fixes the issue where users had to manually navigate to About > Check for Updates to discover new releases.
- **Release Script Compatibility**: Improves release script output encoding and launch behavior for more reliable VS Code Run and Debug execution.

## Technical Changes
- Extended settings persistence to store last attempted update check time and last known update availability metadata.
- Added main-window scheduling logic for throttled background update polling and inline notice state management.
- Updated VS Code launch configuration to use terminal-based release script execution.
- Refined release workflow and script policy for production-only releases on main and test/dev installer flow on non-main branches.

## Breaking Changes (if any)
- None.

## Installation
- Download and run PlaylistSetup.exe

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in installer)

## Documentation
- Full documentation available at https://playlist.ignyos.com/

