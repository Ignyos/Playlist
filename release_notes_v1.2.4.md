# Release v1.2.4

## Overview
Critical stability fix resolving persistent database initialization failures on fresh installations. This patch eliminates duplicate migration execution that was causing "table does not exist" errors on clean installs.

## Bug Fixes
- **Fresh Installation Database Error**: Fixed critical issue where the application failed to start on clean installations with "table Playlists does not exist" error. The problem was caused by duplicate database migration execution with different DbContext instances, creating race conditions during initialization
- **Build Script Preservation**: Updated release script to preserve debug installer files (timestamped builds) while only cleaning the main release installer

## Technical Changes
- Removed duplicate database migration call from `MainWindow.InitializeDatabase()` - migration now happens exclusively in `App.OnStartup()` before any UI initialization
- Database migration runs once at application startup with proper scope management and verification
- Release build script now only deletes `PlaylistSetup.exe` while preserving all timestamped debug installers
- Eliminated race condition between multiple DbContext instances attempting migrations simultaneously

## Installation
- Download and run `PlaylistSetup.exe`

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in installer)

## Documentation
- Full documentation available at https://playlist.ignyos.com/

