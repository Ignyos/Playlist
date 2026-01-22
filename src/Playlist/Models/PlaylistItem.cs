using System;

namespace Playlist.Models;

public class PlaylistItem
{
    public int Id { get; set; }
    
    public int PlaylistId { get; set; }
    
    public int Ordinal { get; set; }
    
    public string Path { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public DateTime LastPlayed { get; set; }
    
    public int? TimeStamp { get; set; }
    
    public long? Duration { get; set; }
    
    public DateTime? DeleteDate { get; set; }
    
    public Playlist Playlist { get; set; } = null!;
    
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
}
