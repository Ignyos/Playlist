# Release v1.2.3

## Overview
Stability patch release fixing database context lifetime management issue that caused application crashes on startup. All features from v1.2.1 remain available and stable.

## Bug Fixes
- **Application Startup Crash**: Fixed critical ObjectDisposedException that prevented the application from launching after the v1.2.2 database migration fix. The issue was caused by manually disposing dependency-injected DbContext instances, which are scoped and managed by the DI container
- **Database Initialization**: Improved error messaging to include inner exception details for better diagnostics

## Technical Changes
- Removed improper `using` statements around DbContext instances obtained from the factory - these are scoped by the DI container and should not be manually disposed
- DbContext instances now have their lifetime properly managed by the dependency injection container throughout the application
- Enhanced database migration process with explicit scoped contexts and post-migration verification
- Added detailed error messages showing both primary and inner exception information during initialization failures

## Previous Release (v1.2.2) Changes

### Bug Fixes (v1.2.2)
- **Database Migration Failure on Fresh Install**: Fixed critical issue where application failed to start on clean installations with "no such table: Playlists" error
  - Database migration now runs synchronously in App.OnStartup before any UI initialization
  - Added verification query to confirm database tables exist and are accessible
  - Refactored database initialization to use explicit DI scopes

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
