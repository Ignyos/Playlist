using Microsoft.Extensions.DependencyInjection;
using Playlist.Data;

namespace Playlist.Services;

/// <summary>
/// Factory for creating PlaylistDbContext instances
/// </summary>
public interface IPlaylistDbContextFactory
{
    /// <summary>
    /// Create a new PlaylistDbContext instance
    /// </summary>
    PlaylistDbContext CreateDbContext();
}

/// <summary>
/// Implementation of IPlaylistDbContextFactory
/// </summary>
public class PlaylistDbContextFactory : IPlaylistDbContextFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PlaylistDbContextFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public PlaylistDbContext CreateDbContext()
    {
        return _serviceProvider.GetRequiredService<PlaylistDbContext>();
    }
}
