## Overview
This release enhances the media player experience with keyboard navigation support and improves progress tracking accuracy to ensure videos marked as complete display 100% progress without visual artifacts.

## New Features
- **Keyboard Navigation**: Press Enter to play a selected playlist item directly from the main list - no need to double-click
- **Media Player Navigation**: Next and Previous buttons in the media player now navigate through playlist items without stopping playback

## Improvements
- **Progress Bar Visual**: Fixed progress indicator to display completely filled (100% green) when videos finish playing - no grey sliver on the right
- **Progress Accuracy**: Enhanced progress percentage calculation to correctly handle edge cases where playback completes, ensuring 100% is displayed when appropriate
- **Media Completion Tracking**: When playback ends, the app now captures the video duration if it wasn't recorded during initial playback and sets progress to exactly 100%
- **Tooltip Clarity**: Updated progress pill tooltips to display complete information (e.g., "100% complete") instead of just the number
- **Visual Polish**: Added subtle drop shadow effect to progress indicator pill for better depth and visual hierarchy
- **Playback State Management**: Fixed issue where selected playlist item wasn't properly saved when opening media player, improving playback resumption

## Bug Fixes
- **100% Progress Display**: Fixed progress bar showing 99% or leaving a grey sliver when videos complete - now displays fully green at 100%
- **Duration Capture**: Videos that don't report duration during initial playback now properly capture it when playback ends, enabling accurate 100% progress marking
- **Selected Item Tracking**: Fixed issue where the app wasn't remembering which item was selected in a playlist, especially after closing the media player

## Technical Changes
- Added `ProgressToWidthConverter` value converter for accurate progress bar width calculation
- Enhanced `PlaylistItemsListBox_KeyDown` handler to support Enter key for media playback
- Improved media player window initialization to accept playlist items and current index for navigation support
- Enhanced `OnMediaEnded` handler to capture missing duration and use ceiling rounding for timestamp calculation
- Fixed progress percentage calculation with explicit check for timestamp >= duration condition
- Updated XAML to use visual progress fill rectangle instead of text-based percentage display
- Improved database state management during playlist item loading
- Implemented media player window navigation event handling for next/previous playback

## Installation
- Download and run `PlaylistSetup.exe`

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in installer)

## Documentation
- Full documentation available at https://playlist.ignyos.com/

