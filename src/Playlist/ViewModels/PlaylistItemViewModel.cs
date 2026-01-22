using System;

namespace Playlist.ViewModels;

public class PlaylistItemViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private string _path = string.Empty;
    private DateTime _lastPlayed;
    private int? _timeStamp;
    private long? _duration;

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
        set
        {
            if (SetProperty(ref _timeStamp, value))
            {
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }
    }

    public long? Duration
    {
        get => _duration;
        set
        {
            if (SetProperty(ref _duration, value))
            {
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }
    }

    public int ProgressPercentage
    {
        get
        {
            if (Duration == null || Duration == 0)
                return 0;
            
            if (TimeStamp == null || TimeStamp == 0)
                return 0;
            
            // TimeStamp is in seconds, Duration is in milliseconds
            var timeStampMs = TimeStamp.Value * 1000L;
            var percentage = (int)((timeStampMs * 100) / Duration.Value);
            return Math.Min(percentage, 100);
        }
    }

    public string DisplayText => Name;
}
