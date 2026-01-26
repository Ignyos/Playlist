using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Playlist.Data;

namespace Playlist;

/// <summary>
/// Factory for creating DbContext instances for design-time tools (EF migrations)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PlaylistDbContext>
{
    public PlaylistDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlaylistDbContext>();
        
        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Playlist",
            "playlist.db"
        );
        
        var directory = System.IO.Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new PlaylistDbContext(optionsBuilder.Options);
    }
}
