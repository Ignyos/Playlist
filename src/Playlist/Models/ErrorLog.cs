using System;

namespace Playlist.Models;

public class ErrorLog
{
    public int Id { get; set; }
    
    public int? PlaylistId { get; set; }
    
    public int? PlaylistItemId { get; set; }
    
    public DateTime TimeStamp { get; set; }
    
    public string ErrorMessage { get; set; } = string.Empty;
    
    public string StackTrace { get; set; } = string.Empty;
}
