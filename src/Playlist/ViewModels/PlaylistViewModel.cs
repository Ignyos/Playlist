using System;
using Playlist.Models;

namespace Playlist.ViewModels;

public class PlaylistViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private DateTime _created;
    private DateTime _lastPlayed;
    private PlaylistPlaybackMode _playbackMode = PlaylistPlaybackMode.StopAfterCurrent;

    public int Id { get; set; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public DateTime Created
    {
        get => _created;
        set => SetProperty(ref _created, value);
    }

    public DateTime LastPlayed
    {
        get => _lastPlayed;
        set => SetProperty(ref _lastPlayed, value);
    }

    public PlaylistPlaybackMode PlaybackMode
    {
        get => _playbackMode;
        set => SetProperty(ref _playbackMode, value);
    }

    public string DisplayText => Name;
}
