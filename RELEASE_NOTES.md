# Release v1.4.1

## Overview
This release improves playlist state control and history management with faster, clearer actions in the main UI. It also fixes duplicate history logging so playback history is more accurate and easier to trust.

## New Features
- **Manual Playlist State Actions**: Adds playlist context menu actions to move a playlist to **Completed** or back to **Queue** without changing item progress.
- **Clear History Action**: Adds a **Clear History** button in the History window with confirmation so you can quickly reset playback history when needed.

## Improvements
- **Playlist Context Menu UX**: Keeps state actions visible where you already manage playlists, reducing clicks and avoiding extra dialogs.
- **State-Aware Menu Actions**: Disables invalid actions automatically (for example, you cannot move an already completed playlist to Completed again).
- **Cleaner History Window Footer**: Groups History actions in a simple bottom action row for easier access.

## Bug Fixes
- **Duplicate History Entries**: Fixes an issue where a viewed item could create two history rows for one playback session.
- **History Accuracy**: Ensures each playback produces a single history entry, making history timelines and troubleshooting more reliable.

## Technical Changes
- Simplifies playback history logging by removing the duplicate end-of-media history write path.
- Adds a dedicated playlist service method for updating parent playlist completion state only.

## Breaking Changes (if any)
- None.

## Installation
- Download and run `PlaylistSetup.exe`.

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in installer)

## Documentation
- Full documentation: https://playlist.ignyos.com/

