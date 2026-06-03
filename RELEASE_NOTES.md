# Release v1.3.0

## Overview
This release introduces per-playlist playback modes so you can control what happens when a video ends. It adds flexible queue behavior for sequential playback, looping, and shuffle workflows, while keeping setup simple through a dedicated Playback Mode screen.

## New Features
- **Per-Playlist Playback Modes**: Adds playlist-level controls for stop after current item, auto-play next, auto-play next with loop, shuffle continuous, and shuffle play-once.
- **Playback Mode Window**: Adds a dedicated window to configure mode behavior for each playlist.
- **Context Menu Entry**: Adds a direct **Playback Mode** action in the playlist context menu for faster access.
- **Shuffle Play-Once Workflow**: Adds random no-repeat playback that stops after all items have been played once.

## Improvements
- **Immediate Mode Application**: Applies mode changes immediately so the next transition follows the newly selected behavior.
- **Auto-Save Experience**: Saves playback mode changes automatically from the Playback Mode window.
- **Playback Navigation Continuity**: Keeps selected item state synchronized as auto-next and shuffle transitions occur.
- **Planning Documentation Structure**: Organizes roadmap planning into roadmap/roadmap.md with detailed per-item docs in roadmap/items/.

## Bug Fixes
- **Playback Mode Dialog Reselection Error**: Fixes a null-reference path when reloading playlists after closing the Playback Mode dialog.

## Technical Changes
- Added a new playlist playback mode model and persistence path in playlist services and view models.
- Added database schema support for persisted playlist playback mode values.
- Added completion-flow logic for sequential, loop, shuffle continuous, and shuffle play-once transitions.
- Added shuffle play-once session tracking and reset behavior for explicit playback restarts.

## Breaking Changes (if any)
- None.

## Installation
- Download and run `PlaylistSetup.exe`.

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in installer)

## Documentation
- Full documentation available at https://playlist.ignyos.com/
