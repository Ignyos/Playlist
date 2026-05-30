# Release v1.2.8

## Overview
This release adds a quick way to manually mark playlist items as complete. It makes progress tracking faster when you want to finish or skip an item without playing it to the end.

## New Features
- **Mark as Completed Action**: Adds a new right-click menu option on playlist items so you can set an item to 100% complete in one step.

## Improvements
- **Faster Progress Management**: Lets you update completion status directly from the playlist context menu without opening playback controls.
- **Reliable Completion for All Items**: Handles items with missing duration data so the completed state is still applied consistently.

## Bug Fixes
- **Completion State Consistency**: Ensures manually completed items are stored with values that correctly display full progress.

## Technical Changes
- Added a new playlist item command handler in the main window for manual completion.
- Added a dedicated service method to persist completed status using existing progress fields.

## Breaking Changes (if any)
- None.

## Installation
- Download and run PlaylistSetup.exe

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in installer)

## Documentation
- Full documentation available at https://playlist.ignyos.com/

