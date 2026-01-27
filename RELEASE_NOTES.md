# Release v1.2.1

## Overview
This release focuses on improving user settings management and enhancing the visual experience with better progress indicators and drag-and-drop functionality.

## New Features
- **Settings Dialog**: Adds a fully functional settings menu where users can configure application preferences including run on startup and fullscreen behavior
- **Run on Startup**: Configure the application to launch automatically when Windows starts through the new settings dialog
- **Fullscreen Preferences**: Control whether videos automatically enter fullscreen mode when playing

## Improvements
- **Progress Indicators**: Completed items (100%) now display with a green background for instant visual feedback on what you've finished
- **Progress Indicator Size**: Increased progress percentage font size from 10pt to 14pt for better readability
- **Drag-and-Drop Reordering**: Completely redesigned the drag-and-drop system with more accurate drop positioning and visual insertion indicators
- **Smart Playback Resume**: When replaying a completed item (at 100%), playback now automatically starts from the beginning instead of the end
- **UI Layout**: Replaced the playlist name banner with cleaner column headers (Progress, Title) for a more streamlined interface
- **List Selection Styling**: Improved visual feedback with custom colors - selected items use blue (#73aaff), hover effects use light blue (#c9deff)
- **Item Spacing**: Increased padding in list items from 2px to 5px for better touch targets and visual comfort

## Bug Fixes
- **Drag-and-Drop Accuracy**: Fixed issues where items would drop in unexpected positions - the drop indicator now precisely shows where items will be inserted
- **Character Encoding**: Resolved encoding issue causing corrupted characters in some source files
- **Startup Settings**: Fixed race condition where startup preferences weren't applied correctly on first launch

## Technical Changes
- Refactored drag-and-drop adorner system from `InsertionAdorner` to `ListBoxInsertionAdorner` for better positioning accuracy
- Added `StartupService.ApplyRunOnStartup()` for managing Windows startup registry entries
- Improved state tracking during drag operations with `_dragTargetIndex` field
- Enhanced media playback logic to detect completion status before resuming
