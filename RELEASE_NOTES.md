# Release v1.4.0

## Overview
This release focuses on faster day-to-day playback control, clearer playlist organization, and more reliable update detection. It adds quality-of-life features for managing progress and startup behavior while improving how the app identifies the newest installable version.

## New Features
- **Queue and Completed Playlist Sections**: Adds persistent playlist states so active and completed playlists are shown in separate sections for easier organization.
- **Playlist Double-Click Playback**: Double-clicking a playlist now starts playback automatically using that playlist's selected playback mode.
- **Default Playback Mode Setting**: Adds a new Settings option to choose the default playback mode applied to newly created playlists.
- **Mark as Unwatched Action**: Adds a playlist item context-menu action to reset an item's progress back to zero.

## Improvements
- **Playback Mode Editing Flow**: Replaces the old Playback Mode dialog with a direct context-menu submenu so users can change modes in fewer clicks.
- **Playback Mode Guidance**: Adds mode descriptions as tooltips directly in the Playback Mode submenu.
- **Playlist Sorting Behavior**: Orders playlists alphabetically within each section (incomplete first, completed second) for more predictable scanning.
- **Cleaner Playlist Context Menu**: Streamlines the playlist context menu to the core actions: Edit, Playback Mode, and Remove.

## Bug Fixes
- **Update Detection Accuracy**: Fixes cases where update checks could miss the most recent installable release when multiple releases are available.
- **Stale Version Notice Handling**: Fixes scenarios where the app could still show an update notice after you already installed a newer version.
- **Inline Update Download Freshness**: Fixes cases where the update notice could use stale cached data and download an older installer URL.

## Technical Changes
- **Release Selection Logic**: Update checks now evaluate a broader release list and choose the newest valid installer-backed release.
- **Data Model Updates**: Adds playlist state fields to support sectioned playlist organization.
- **Legacy UI Cleanup**: Removes obsolete Playback Mode window files after migrating to in-menu mode selection.

## Breaking Changes (if any)
- None.

## Installation
- Download and run `PlaylistSetup.exe`.

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in installer)

## Documentation
- Full documentation: https://playlist.ignyos.com/

