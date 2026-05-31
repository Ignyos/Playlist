using Microsoft.Extensions.DependencyInjection;
using Playlist.Data;
using Microsoft.EntityFrameworkCore;

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
    private readonly IDbContextFactory<PlaylistDbContext> _dbContextFactory;

    public PlaylistDbContextFactory(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
        _dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<PlaylistDbContext>>();
    }

    public PlaylistDbContext CreateDbContext()
    {
        return _dbContextFactory.CreateDbContext();
    }
}
