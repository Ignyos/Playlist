using System;

namespace Playlist.Models;

public class History
{
    public int Id { get; set; }
    
    public int PlaylistId { get; set; }
    
    public int PlaylistItemId { get; set; }
    
    public DateTime TimeStamp { get; set; }
    
    public Playlist Playlist { get; set; } = null!;
    
    public PlaylistItem PlaylistItem { get; set; } = null!;
}
