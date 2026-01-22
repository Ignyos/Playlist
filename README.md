# Playlist

<p align="center">
  <img src="src/Playlist/Assets/icon.svg" alt="Playlist Logo" width="128" height="128">
</p>

A Windows desktop application for managing and playing playlists of video files with integrated VLC media player.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)

## Features

- 📋 **Playlist Management** - Create, edit, and organize multiple playlists
- 🎬 **Embedded Video Player** - Integrated VLC media player with overlay controls
- ⏯️ **Resume Playback** - Automatically saves playback position
- 📊 **Playback History** - Track what you've watched and when
- 🎨 **Modern UI** - Clean WPF interface with auto-hiding controls
- 🔍 **Search & Sort** - Find and organize playlists easily
- 💾 **SQLite Database** - Local storage with no cloud dependencies

## Requirements

- **Windows 10/11** (64-bit)
- **.NET 9.0 Runtime** (included in installer)
- **VLC Libraries** (included in installer)

## Installation

### Option 1: Windows Installer (Recommended)

1. Download the latest installer from [Releases](https://github.com/Ignyos/Playlist/releases)
2. Run `PlaylistSetup.exe`
3. Follow the installation wizard
4. Launch Playlist from the Start Menu

### Option 2: Build from Source

```bash
# Clone the repository
git clone https://github.com/Ignyos/Playlist.git
cd Playlist

# Restore dependencies
dotnet restore

# Build the application
dotnet build --configuration Release

# Run the application
cd src/Playlist
dotnet run
```

## Technical Details

### Built With

- **WPF** - Windows Presentation Foundation UI framework
- **LibVLCSharp** - .NET wrapper for VLC media player
- **Entity Framework Core** - ORM for database access
- **SQLite** - Embedded database for local storage

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [LibVLCSharp](https://github.com/videolan/libvlcsharp) - VLC media player integration
- [VideoLAN](https://www.videolan.org/) - VLC media player
- [Entity Framework Core](https://github.com/dotnet/efcore) - Database ORM

---

Made with ❤️ by [Ignyos](https://ignyos.com)
