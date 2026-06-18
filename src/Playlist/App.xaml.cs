using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Data.Sqlite;
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
        // Configure and build the dependency injection container
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Run database migrations with a dedicated DbContext
        try
        {
            var dbFactory = _serviceProvider.GetRequiredService<IDbContextFactory<PlaylistDbContext>>();
            InitializeDatabase(dbFactory);
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

        // Let WPF create the startup window only after the database is ready.
        base.OnStartup(e);
    }

    private static void InitializeDatabase(IDbContextFactory<PlaylistDbContext> dbFactory)
    {
        using var dbContext = dbFactory.CreateDbContext();

        System.Diagnostics.Debug.WriteLine("Starting database migration...");
        dbContext.Database.Migrate();
        System.Diagnostics.Debug.WriteLine("Database migration completed");

        using var connection = dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        var repaired = EnsureSchemaInPlace(connection);
        if (repaired)
        {
            System.Diagnostics.Debug.WriteLine("Database schema repaired in place.");
        }

        if (!HasRequiredSchema(connection))
        {
            throw new InvalidOperationException("Database schema verification failed: required tables/columns are still missing after repair.");
        }

        connection.Close();

        var tableCount = dbContext.Playlists.Count();
        System.Diagnostics.Debug.WriteLine($"Database verification successful. Playlist count: {tableCount}");
    }

    private static bool EnsureSchemaInPlace(System.Data.Common.DbConnection connection)
    {
        var changed = false;

        changed |= EnsureTableExists(connection, "Playlists", @"
CREATE TABLE IF NOT EXISTS Playlists (
    Id INTEGER NOT NULL CONSTRAINT PK_Playlists PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Created TEXT NOT NULL,
    LastPlayed TEXT NOT NULL,
    SelectedItemId INTEGER NULL,
    DeleteDate TEXT NULL,
    PlaybackMode INTEGER NOT NULL DEFAULT 0,
    IsCompleted INTEGER NOT NULL DEFAULT 0,
    QueueOrder INTEGER NULL
);");

        changed |= EnsureTableExists(connection, "PlaylistItems", @"
CREATE TABLE IF NOT EXISTS PlaylistItems (
    Id INTEGER NOT NULL CONSTRAINT PK_PlaylistItems PRIMARY KEY AUTOINCREMENT,
    PlaylistId INTEGER NOT NULL,
    Ordinal INTEGER NOT NULL,
    Path TEXT NOT NULL,
    Name TEXT NOT NULL,
    LastPlayed TEXT NOT NULL,
    TimeStamp INTEGER NULL,
    Duration INTEGER NULL,
    DeleteDate TEXT NULL,
    CONSTRAINT FK_PlaylistItems_Playlists_PlaylistId FOREIGN KEY (PlaylistId) REFERENCES Playlists (Id) ON DELETE CASCADE
);");

        changed |= EnsureTableExists(connection, "Settings", @"
CREATE TABLE IF NOT EXISTS Settings (
    Key TEXT NOT NULL CONSTRAINT PK_Settings PRIMARY KEY,
    Value TEXT NOT NULL
);");

        changed |= EnsureTableExists(connection, "History", @"
CREATE TABLE IF NOT EXISTS History (
    Id INTEGER NOT NULL CONSTRAINT PK_History PRIMARY KEY AUTOINCREMENT,
    PlaylistId INTEGER NOT NULL,
    PlaylistItemId INTEGER NOT NULL,
    TimeStamp TEXT NOT NULL,
    CONSTRAINT FK_History_Playlists_PlaylistId FOREIGN KEY (PlaylistId) REFERENCES Playlists (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_History_PlaylistItems_PlaylistItemId FOREIGN KEY (PlaylistItemId) REFERENCES PlaylistItems (Id) ON DELETE RESTRICT
);");

        changed |= EnsureTableExists(connection, "ErrorLogs", @"
CREATE TABLE IF NOT EXISTS ErrorLogs (
    Id INTEGER NOT NULL CONSTRAINT PK_ErrorLogs PRIMARY KEY AUTOINCREMENT,
    PlaylistId INTEGER NULL,
    PlaylistItemId INTEGER NULL,
    TimeStamp TEXT NOT NULL,
    ErrorMessage TEXT NOT NULL,
    StackTrace TEXT NOT NULL
);");

        changed |= EnsureColumnExists(connection, "Playlists", "PlaybackMode", "INTEGER NOT NULL DEFAULT 0");
        changed |= EnsureColumnExists(connection, "Playlists", "IsCompleted", "INTEGER NOT NULL DEFAULT 0");
        changed |= EnsureColumnExists(connection, "Playlists", "QueueOrder", "INTEGER NULL");
        changed |= EnsureColumnExists(connection, "Playlists", "SelectedItemId", "INTEGER NULL");
        changed |= EnsureColumnExists(connection, "Playlists", "DeleteDate", "TEXT NULL");

        changed |= EnsureIndexExists(connection, "IX_PlaylistItems_PlaylistId", "CREATE INDEX IF NOT EXISTS IX_PlaylistItems_PlaylistId ON PlaylistItems (PlaylistId);");
        changed |= EnsureIndexExists(connection, "IX_History_PlaylistId", "CREATE INDEX IF NOT EXISTS IX_History_PlaylistId ON History (PlaylistId);");
        changed |= EnsureIndexExists(connection, "IX_History_PlaylistItemId", "CREATE INDEX IF NOT EXISTS IX_History_PlaylistItemId ON History (PlaylistItemId);");

        return changed;
    }

    private static bool HasRequiredSchema(System.Data.Common.DbConnection connection)
    {
        var requiredColumns = new Dictionary<string, string[]>
        {
            ["Playlists"] = ["Id", "Name", "Created", "LastPlayed", "SelectedItemId", "DeleteDate", "PlaybackMode", "IsCompleted", "QueueOrder"],
            ["PlaylistItems"] = ["Id", "PlaylistId", "Ordinal", "Path", "Name", "LastPlayed", "TimeStamp", "Duration", "DeleteDate"],
            ["Settings"] = ["Key", "Value"],
            ["History"] = ["Id", "PlaylistId", "PlaylistItemId", "TimeStamp"],
            ["ErrorLogs"] = ["Id", "PlaylistId", "PlaylistItemId", "TimeStamp", "ErrorMessage", "StackTrace"]
        };

        foreach (var kvp in requiredColumns)
        {
            if (!TableExists(connection, kvp.Key))
            {
                return false;
            }

            var actualColumns = GetColumns(connection, kvp.Key);
            foreach (var expectedColumn in kvp.Value)
            {
                if (!actualColumns.Contains(expectedColumn))
                {
                    return false;
                }
            }
        }

        return IndexExists(connection, "IX_PlaylistItems_PlaylistId")
            && IndexExists(connection, "IX_History_PlaylistId")
            && IndexExists(connection, "IX_History_PlaylistItemId");
    }

    private static bool EnsureTableExists(System.Data.Common.DbConnection connection, string tableName, string createSql)
    {
        if (TableExists(connection, tableName))
        {
            return false;
        }

        ExecuteNonQuery(connection, createSql);
        return true;
    }

    private static bool EnsureColumnExists(System.Data.Common.DbConnection connection, string tableName, string columnName, string columnSql)
    {
        if (!TableExists(connection, tableName))
        {
            return false;
        }

        var columns = GetColumns(connection, tableName);
        if (columns.Contains(columnName))
        {
            return false;
        }

        ExecuteNonQuery(connection, $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnSql};");
        return true;
    }

    private static bool EnsureIndexExists(System.Data.Common.DbConnection connection, string indexName, string createIndexSql)
    {
        if (IndexExists(connection, indexName))
        {
            return false;
        }

        ExecuteNonQuery(connection, createIndexSql);
        return true;
    }

    private static HashSet<string> GetColumns(System.Data.Common.DbConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName});";

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static bool IndexExists(System.Data.Common.DbConnection connection, string indexName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name = $name;";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "$name";
        parameter.Value = indexName;
        command.Parameters.Add(parameter);

        var result = command.ExecuteScalar();
        return Convert.ToInt32(result) > 0;
    }

    private static void ExecuteNonQuery(System.Data.Common.DbConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static bool HasRequiredTables(PlaylistDbContext dbContext)
    {
        try
        {
            using var connection = dbContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return HasRequiredSchema(connection);
        }
        catch (SqliteException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Schema verification failed with SQLite exception: {ex.Message}");
            return false;
        }
    }

    private static bool TableExists(System.Data.Common.DbConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name = $name;";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "$name";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var result = command.ExecuteScalar();
        return Convert.ToInt32(result) > 0;
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

        // Register DbContext factory with SQLite
        services.AddDbContextFactory<PlaylistDbContext>(options =>
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


