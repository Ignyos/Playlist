using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace Playlist.Models;

public class Setting
{
    [Key]
    public string Key { get; set; } = string.Empty;
    
    public string Value { get; set; } = string.Empty;
}
