# Release v1.3.1

## Overview
This release focuses on stability and recovery for playback mode workflows and startup database initialization. It also improves local development testing setup with scenario-based launch profiles in VS Code.

## New Features
- **Database Self-Repair on Startup**: Adds in-place schema repair that can create missing core tables, columns, and indexes when a local database is incomplete.
- **VS Code Scenario Launch Profiles**: Adds one-click launch profiles for normal run, clean/new database testing, and existing database upgrade testing.

## Improvements
- **Safer Database Recovery**: Improves startup behavior to repair missing schema elements in place instead of deleting the whole database by default.
- **Developer Testing Workflow**: Adds reusable backup/remove database tasks and scenario orchestration tasks to speed regression testing.
- **Testing Documentation**: Adds a dedicated development testing checklist with scenario guidance and verification points.

## Bug Fixes
- **Playback Mode Threading Crash**: Fixes cross-thread UI access in playback continuation flow when using modes other than Stop after current item.
- **Media Ended Continuation Safety**: Wraps async media-ended continuation with exception safety to prevent process-terminating unhandled exceptions.
- **Startup Initialization Failures**: Fixes startup failures caused by missing Playlists and related schema objects in partially initialized local databases.

## Technical Changes
- Added dispatcher marshaling for UI-touching operations in playback auto-advance flow.
- Added guarded async error handling in media-ended event continuation.
- Added startup schema verification and repair helpers for required tables, columns, and indexes.
- Added VS Code tasks for local DB backup/reset and scenario-driven launch orchestration.

## Breaking Changes (if any)
- None.

## Installation
- Download and run `PlaylistSetup.exe`.

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in installer)

## Documentation
- Full documentation available at https://playlist.ignyos.com/
