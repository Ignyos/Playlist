# Release v1.2.7

## Overview
This release fixes update detection so Check for Updates reports the correct result.

## New Features
- **Version Parsing**: Update checks now handle release tag formats more reliably.

## Improvements
- **Update Messages**: Parsing failures now show a clear error instead of a false up-to-date message.
- **Version Display**: Current and latest version values are shown in a consistent format.

## Bug Fixes
- **Check for Updates**: Fixed an issue where the app could always report "latest version".

## Technical Changes
- Removed obsolete helper and migration scripts.
- Simplified update-service version comparison logic.

## Breaking Changes (if any)
- None.

## Installation
- Download and run `PlaylistSetup.exe`

## Requirements
- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in installer)

## Documentation
- Full documentation available at https://playlist.ignyos.com/

