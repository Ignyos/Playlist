# Release v1.4.3

## Overview
This release improves playlist playback flow so double-clicking a playlist starts the logical next video for the selected playback mode. It also adds a faster way to reset watch progress for an entire playlist.

## New Features
- **Mark All Unwatched**: Adds a new playlist context menu action to reset all items in a playlist to unwatched in one click.

## Improvements
- **Playlist Double-Click Behavior**: Starts playback using mode-aware next-item logic instead of always starting the currently selected item.
- **Smart Playback Anchor Selection**: Uses the saved selected/last-played item when available, and otherwise falls back to the most relevant partially viewed item.
- **Sequential Start Consistency**: Starts from the next playable item and wraps to the beginning when needed to avoid dead-end starts.
- **Shuffle Continuous Start Quality**: Prefers a random item that is different from the anchor when possible for better variety.
- **Shuffle Play-Once Continuity**: Preserves the active play-once session when starting from playlist double-click so previously played items are respected.

## Bug Fixes
- **Playlist Double-Click No-Op Cases**: Fixes scenarios where start behavior could feel incorrect by consistently choosing a logical playable start item per mode.
- **Shuffle Play-Once Session Reset Behavior**: Fixes unintended session resets during playlist-driven playback starts, reducing unexpected repeats.

## Technical Changes
- Added helper logic for playback anchor resolution, partial-progress evaluation, sequential next-item selection, and shuffle candidate selection.
- Added a bulk playlist service operation to clear timestamps for all active items in a playlist.
- Updated media start call flow to optionally preserve shuffle play-once session state for mode-specific starts.

## Breaking Changes
- None.

## Installation
- Download and run `PlaylistSetup.exe`.

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in installer)

## Documentation
- Full documentation available at https://playlist.ignyos.com/

