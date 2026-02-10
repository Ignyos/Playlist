## Overview
Critical stability fix addressing a database initialization issue that prevented the application from starting on fresh installations. This patch ensures all database tables are properly created before the application attempts to use them.

## Bug Fixes
- **Fresh Installation Database Error**: Fixed critical issue where application failed to start on clean installations with error "no such table: Playlists". Database migration now runs synchronously during application startup, ensuring all tables exist before any data access operations begin

## Improvements
- **UI Layout**: Replaced the playlist name banner with cleaner column headers (Progress, Title) for a more streamlined and consistent interface
- **Progress Indicators**: Completed items (100%) now display with a green background (#C8E6C9) for instant visual feedback on what you've finished
- **Progress Indicator Size**: Increased progress percentage font size from 10pt to 14pt for better readability
- **List Item Spacing**: Increased padding in list items from 2px to 5px for better touch targets and visual comfort
- **Selection Styling**: Improved visual feedback with custom colors - selected items use blue (#73aaff) with white text, hover effects use light blue (#c9deff)
- **Visual Polish**: Hidden the grid splitter between playlists and items for a cleaner interface appearance

## New Features
- **Settings Menu**: New Settings option in the File menu opens a configuration dialog for application preferences
- **Run on Startup**: Configure the application to launch automatically when Windows starts through the new settings dialog
- **Fullscreen Preference**: Control whether videos automatically enter fullscreen mode when playing
- **Drag-and-Drop Reordering**: Completely redesigned drag-and-drop system with visual insertion indicators showing exactly where items will be placed

## Technical Changes
- Refactored database initialization in App startup to use synchronous migration with dedicated DbContext scope
- Removed async migration pattern that could leave database tables uninitialized during application startup
- Enhanced drag-and-drop adorner system with `ListBoxInsertionAdorner` for precise visual feedback
- Added `_dragTargetIndex` field for accurate drop position tracking
- Improved bounds calculation for drag-and-drop targeting to handle all list positions correctly
- Added startup preference application in `ApplyRunOnStartupSetting()` method
- Settings are now loaded and applied when the application starts

## Installation
- Download and run `PlaylistSetup.exe`

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in installer)

## Documentation
- Full documentation available at https://playlist.ignyos.com/

