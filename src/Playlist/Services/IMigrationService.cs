using System.Threading.Tasks;

namespace Playlist.Services;

/// <summary>
/// Service for managing database migrations
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Apply all pending migrations to the database
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task MigrateAsync();
}
