using System.IO;
using Microsoft.EntityFrameworkCore;
using Playlist.Models;

namespace Playlist.Data;

public class PlaylistDbContext : DbContext
{
    public DbSet<Setting> Settings { get; set; } = null!;
    public DbSet<Models.Playlist> Playlists { get; set; } = null!;
    public DbSet<PlaylistItem> PlaylistItems { get; set; } = null!;
    public DbSet<History> History { get; set; } = null!;
    public DbSet<ErrorLog> ErrorLogs { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Playlist",
            "playlist.db");
        
        var directory = System.IO.Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        optionsBuilder.UseSqlite($"Data Source={dbPath}")
            .LogTo(message => System.Diagnostics.Debug.WriteLine(message), Microsoft.Extensions.Logging.LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Setting
        modelBuilder.Entity<Setting>()
            .HasKey(s => s.Key);

        // Configure Playlist
        modelBuilder.Entity<Models.Playlist>()
            .HasMany(p => p.Items)
            .WithOne(pi => pi.Playlist)
            .HasForeignKey(pi => pi.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure History
        modelBuilder.Entity<History>()
            .HasOne(h => h.Playlist)
            .WithMany()
            .HasForeignKey(h => h.PlaylistId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<History>()
            .HasOne(h => h.PlaylistItem)
            .WithMany()
            .HasForeignKey(h => h.PlaylistItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
