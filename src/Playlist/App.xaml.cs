using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Playlist.Data;
using Playlist.Services;

namespace Playlist;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure and build the dependency injection container
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Run database migrations with a dedicated DbContext
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PlaylistDbContext>();
                System.Diagnostics.Debug.WriteLine("Starting database migration...");
                dbContext.Database.Migrate();
                System.Diagnostics.Debug.WriteLine("Database migration completed");
            }
            
            // Verify database was created by checking if we can query a table
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PlaylistDbContext>();
                var tableCount = dbContext.Playlists.Count();
                System.Diagnostics.Debug.WriteLine($"Database verification successful. Playlist count: {tableCount}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize the database: {ex.Message}\n\n{ex.InnerException?.Message}",
                "Initialization Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            Shutdown(1);
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Get the database path
        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Playlist",
            "playlist.db"
        );

        // Create directory if it doesn't exist
        var directory = System.IO.Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        // Register DbContext with SQLite
        services.AddDbContext<PlaylistDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}")
                .LogTo(message => System.Diagnostics.Debug.WriteLine(message), 
                    Microsoft.Extensions.Logging.LogLevel.Information)
        );

        // Register DbContext factory
        services.AddSingleton<IPlaylistDbContextFactory>(provider => new PlaylistDbContextFactory(provider));

        // Register setting service
        services.AddSingleton<ISettingService, SettingService>();
    }

    public IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("ServiceProvider not initialized");
}


