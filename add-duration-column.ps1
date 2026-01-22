# Simple script to add Duration column to existing database
$dbPath = Join-Path $env:LOCALAPPDATA "Playlist\playlist.db"

if (Test-Path $dbPath) {
    Write-Host "Found database at: $dbPath"
    Write-Host "Adding Duration column using dotnet..."
    
    # Use dotnet to execute raw SQL
    $sql = "ALTER TABLE PlaylistItems ADD COLUMN Duration INTEGER;"
    
    # Create a temporary connection string file
    $tempCs = "Data Source=$dbPath"
    
    # Execute using SQLite command via .NET
    Add-Type -AssemblyName System.Data
    $connection = New-Object System.Data.SQLite.SQLiteConnection
    $connection.ConnectionString = $tempCs
    
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = "ALTER TABLE PlaylistItems ADD COLUMN Duration INTEGER;"
        $command.ExecuteNonQuery() | Out-Null
        Write-Host "✓ Duration column added successfully!"
        
        # Also update the migration history
        $migrationCmd = $connection.CreateCommand()
        $migrationCmd.CommandText = "INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260122010830_AddDurationToPlaylistItem', '9.0.0');"
        $migrationCmd.ExecuteNonQuery() | Out-Null
        Write-Host "✓ Migration history updated!"
    }
    catch {
        Write-Host "❌ Error: $_"
        Write-Host ""
        Write-Host "The column might already exist, or you need System.Data.SQLite installed."
        Write-Host "Easiest solution: Delete the database and let it recreate:"
        Write-Host ""
        Write-Host "  Remove-Item '$dbPath'"
        Write-Host ""
    }
    finally {
        if ($connection.State -eq 'Open') {
            $connection.Close()
        }
    }
} else {
    Write-Host "Database not found at $dbPath"
    Write-Host "It will be created automatically when you run the app."
}
