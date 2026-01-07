# Changelog

All notable changes to Playlist will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-01-07

### Added
- Initial release of Playlist application
- Playlist management with create, edit, delete functionality
- Integrated VLC media player with LibVLCSharp
- Auto-hiding overlay controls with fade animations
- Full-screen mode with taskbar hiding
- Resume playback from saved timestamps
- Playback history tracking
- Search and filter playlists by name
- Sort playlists by name, creation date, or last played
- Drag-and-drop reordering of playlist items
- Right-click context menus for playlists and items
- Custom item naming without affecting source files
- Smart merge when editing playlists (preserves customizations)
- SQLite database for local storage
- Progress slider with seek functionality
- Volume control
- Keyboard shortcuts (Space, F11, Escape)
- Play/Pause/Stop controls
- Time display (current/duration)
- Mouse movement detection for control visibility

### Technical
- Built with .NET 9.0 and WPF
- Entity Framework Core 9.0 with SQLite
- LibVLCSharp 3.9.5 for media playback
- VideoLAN.LibVLC.Windows 3.0.21
- Database migrations with EF Core
- Soft-delete pattern for data integrity
- MVVM architecture
- MediaPlayerService for centralized playback control
- Error logging to database

### Known Issues
- None at release

[1.0.0]: https://github.com/Ignyos/Playlist/releases/tag/v1.0.0
