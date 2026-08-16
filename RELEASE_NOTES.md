# Release v1.4.4

## Overview
Playlist playback is more reliable in this release. It improves how items are selected and started, reduces accidental reordering, and fixes progress tracking so partially watched videos resume and display correctly.

## New Features
- **No major new features**: This release focuses on playback reliability and behavior fixes rather than adding new user-facing commands.

## Improvements
- **Playlist start selection**: Playlist playback now follows a more consistent start rule by respecting item order and choosing the next sensible item when a list contains both watched and unwatched videos.
- **Drag-and-drop behavior**: Reordering is now more precise because dragging only starts from an actual playlist item, which makes accidental moves less likely.
- **Playback stop flow**: Stopping or closing the player now uses the current visible playback position, which makes saved progress more accurate.

## Bug Fixes
- **Incorrect 100% progress after stopping**: Fixed an issue where videos could appear fully watched even when playback stopped early, by refreshing stale stored duration values when the real media length is known.
- **Accidental playlist reordering**: Fixed a bug where clicking empty space or interacting with a context menu could still trigger a drag operation and reorder items unexpectedly.
- **Closing the media player**: Fixed playback shutdown behavior so the player closes cleanly without race conditions that could interfere with saved progress.

## Technical Changes
- Refined playlist item selection logic to use ordinal ordering and shared progress-aware rules across playback modes.
- Added safeguards in playback shutdown handling to ignore late media-end events while stop/close cleanup is in progress.
- Updated stored duration handling so synthetic completion values are replaced with real measured media lengths whenever possible.

## Breaking Changes
- None.

## Installation
- Download and run `PlaylistSetup.exe`.

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in the installer)

## Documentation
- Full documentation is available at https://playlist.ignyos.com/

