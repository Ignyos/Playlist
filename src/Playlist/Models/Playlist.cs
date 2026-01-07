using System;
using System.Collections.Generic;

namespace Playlist.Models;

public class Playlist
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public DateTime Created { get; set; }
    
    public DateTime LastPlayed { get; set; }
    
    public int? SelectedItemId { get; set; }
    
    public DateTime? DeleteDate { get; set; }
    
    public ICollection<PlaylistItem> Items { get; set; } = new List<PlaylistItem>();
}
