using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;

namespace Playlist.Services;

/// <summary>
/// Service for managing database migrations
/// </summary>
public class MigrationService : IMigrationService
{
    private readonly PlaylistDbContext _dbContext;

    public MigrationService(PlaylistDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Apply all pending migrations to the database
    /// </summary>
    public async Task MigrateAsync()
    {
        try
        {
            await _dbContext.Database.MigrateAsync();
            System.Diagnostics.Debug.WriteLine("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying migrations: {ex.Message}");
            throw;
        }
    }
}
