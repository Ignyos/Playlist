using System;

namespace Playlist.ViewModels;

public class PlaylistViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private DateTime _created;
    private DateTime _lastPlayed;

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

    public string DisplayText => Name;
}
