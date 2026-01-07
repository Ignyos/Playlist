using System;

namespace Playlist.ViewModels;

public class PlaylistItemViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private string _path = string.Empty;
    private DateTime _lastPlayed;
    private int? _timeStamp;

    public int Id { get; set; }
    public int PlaylistId { get; set; }
    public int Ordinal { get; set; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }

    public DateTime LastPlayed
    {
        get => _lastPlayed;
        set => SetProperty(ref _lastPlayed, value);
    }

    public int? TimeStamp
    {
        get => _timeStamp;
        set => SetProperty(ref _timeStamp, value);
    }

    public string DisplayText => Name;
}
