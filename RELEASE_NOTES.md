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

## Previous Release (v1.2.3) Changes

### Bug Fixes (v1.2.3)
- **Application Startup Crash**: Fixed ObjectDisposedException that prevented the application from launching after the v1.2.2 database migration fix
- **Database Initialization**: Improved error messaging to include inner exception details for better diagnostics
- Removed improper `using` statements around DbContext instances - now properly managed by the DI container

## Previous Release (v1.2.2) Changes

### Bug Fixes (v1.2.2)
- **Database Migration Failure on Fresh Install**: Fixed issue where application failed to start on clean installations with "no such table: Playlists" error
  - Database migration runs synchronously in App.OnStartup before any UI initialization
  - Added verification query to confirm database tables exist and are accessible

## Previous Release (v1.2.1) Features

### New Features
- **Settings Dialog**: Fully functional settings menu where users can configure application preferences including run on startup and fullscreen behavior
- **Run on Startup**: Configure the application to launch automatically when Windows starts through the new settings dialog
- **Fullscreen Preferences**: Control whether videos automatically enter fullscreen mode when playing

### Improvements
- **Progress Indicators**: Completed items (100%) now display with a green background for instant visual feedback on what you've finished
- **Progress Indicator Size**: Increased progress percentage font size from 10pt to 14pt for better readability
- **Drag-and-Drop Reordering**: Completely redesigned the drag-and-drop system with more accurate drop positioning and visual insertion indicators
- **Smart Playback Resume**: When replaying a completed item (at 100%), playback now automatically starts from the beginning instead of the end
- **UI Layout**: Replaced the playlist name banner with cleaner column headers (Progress, Title) for a more streamlined interface
- **List Selection Styling**: Improved visual feedback with custom colors - selected items use blue (#73aaff), hover effects use light blue (#c9deff)
- **Item Spacing**: Increased padding in list items from 2px to 5px for better touch targets and visual comfort

### Bug Fixes (v1.2.1)
- **Drag-and-Drop Accuracy**: Fixed issues where items would drop in unexpected positions - the drop indicator now precisely shows where items will be inserted
- **Character Encoding**: Resolved encoding issue causing corrupted characters in some source files
- **Startup Settings**: Fixed race condition where startup preferences weren't applied correctly on first launch

### Technical Details (v1.2.1)
- Refactored drag-and-drop adorner system from `InsertionAdorner` to `ListBoxInsertionAdorner` for better positioning accuracy
- Added `StartupService.ApplyRunOnStartup()` for managing Windows startup registry entries
- Improved state tracking during drag operations with `_dragTargetIndex` field
- Enhanced media playback logic to detect completion status before resuming
