# Release v1.2.10

## Overview
This release focuses on playback stability and smoother everyday interactions. Playlist now handles database activity more safely during playback, improves playlist right-click behavior, and captures playback failures more reliably for faster troubleshooting.

## New Features
- **Smart Playlist Right-Click Menu**: Adds context-aware behavior in the Playlists panel so right-clicking empty space now shows a dedicated **New Playlist** action.
- **In-App Playback Error Logging**: Adds direct playback error logging so failures are recorded more consistently for diagnostics and support.

## Improvements
- **Playback Reliability Architecture**: Improves database access patterns by using short-lived context instances for operations, reducing contention during active playback.
- **App Startup Readiness**: Updates startup flow so the main window opens only after database initialization is complete, improving launch consistency.
- **Data Access Consistency**: Updates playlist and media operations to use safer context lifetime handling across key UI workflows.

## Bug Fixes
- **Volume/Playback Concurrency Crash**: Fixes the issue where adjusting volume during playback could trigger a DbContext concurrency exception and terminate the app.
- **Playlist Context Menu Mismatch**: Fixes left-panel behavior so item actions appear only when right-clicking a playlist item, while empty-space right-click now shows only creation actions.
- **Missed Playback Error Records**: Fixes cases where playback exceptions were not consistently captured in application logs.

## Technical Changes
- Switched to EF Core DbContext factory-based creation for safer per-operation database usage.
- Added throttled and synchronized timestamp persistence logic in media playback time-change handling.
- Updated service and window-level database call sites to dispose contexts deterministically.

## Breaking Changes (if any)
- None.

## Installation
- Download and run PlaylistSetup.exe

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in installer)

## Documentation
- Full documentation available at https://playlist.ignyos.com/

