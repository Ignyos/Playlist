# Release v1.2.2

## Overview
Critical bug fix release addressing a database initialization issue that prevented the application from starting on fresh installations. Includes all features and improvements from v1.2.1.

## Bug Fixes
- **Database Migration Failure**: Fixed critical issue where application failed to start on clean installs with error "no such table: Playlists" or "disposed context instance". Root causes and solutions:
  - Database migration now runs synchronously in App.OnStartup before any UI initialization
  - Added verification query to confirm tables exist before proceeding
  - Fixed DbContext disposal pattern: contexts from the factory are scoped and managed by the DI container; no longer manually disposed which was causing ObjectDisposedException
  - Removed improper `using` statements around contexts obtained from the factory to respect DI container lifetime management

## Technical Changes
- Refactored database initialization in `App.xaml.cs` to run migrations synchronously in OnStartup with verification via explicit DI scopes
- Added post-migration verification query to confirm database tables are accessible
- Fixed DbContext lifetime management in `MainWindow.xaml.cs`: removed improper `using` statements that were disposing scoped contexts prematurely
- DbContext instances obtained from the factory are now left for the DI container to manage their disposal, preventing ObjectDisposedException
- Database migration now completes and validates before any UI components attempt to load data

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
