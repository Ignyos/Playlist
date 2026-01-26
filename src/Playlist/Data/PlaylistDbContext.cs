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

    public PlaylistDbContext(DbContextOptions<PlaylistDbContext> options) 
        : base(options)
    {
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
