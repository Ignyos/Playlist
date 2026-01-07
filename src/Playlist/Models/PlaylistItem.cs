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
    
    public DateTime? DeleteDate { get; set; }
    
    public Playlist Playlist { get; set; } = null!;
}
