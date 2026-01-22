# Script to manually add Duration column to existing database
$dbPath = Join-Path $env:LOCALAPPDATA "Playlist\playlist.db"

Write-Host "Database path: $dbPath"

if (Test-Path $dbPath) {
    Write-Host "Database found, adding Duration column..."
    
    # Load SQLite assembly
    Add-Type -Path "C:\Users\$env:USERNAME\.nuget\packages\microsoft.data.sqlite.core\9.0.0\lib\net8.0\Microsoft.Data.Sqlite.dll"
    
    # Open connection
    $connectionString = "Data Source=$dbPath"
    $connection = New-Object Microsoft.Data.Sqlite.SqliteConnection($connectionString)
    $connection.Open()
    
    try {
        # Check if column already exists
        $checkCmd = $connection.CreateCommand()
        $checkCmd.CommandText = "PRAGMA table_info(PlaylistItems);"
        $reader = $checkCmd.ExecuteReader()
        
        $columnExists = $false
        while ($reader.Read()) {
            if ($reader["name"] -eq "Duration") {
                $columnExists = $true
                break
            }
        }
        $reader.Close()
        
        if (-not $columnExists) {
            Write-Host "Adding Duration column..."
            $cmd = $connection.CreateCommand()
            $cmd.CommandText = "ALTER TABLE PlaylistItems ADD COLUMN Duration INTEGER;"
            $cmd.ExecuteNonQuery() | Out-Null
            Write-Host "Duration column added successfully!"
        } else {
            Write-Host "Duration column already exists."
        }
        
        # Update migration history
        Write-Host "Updating migration history..."
        $migrationCmd = $connection.CreateCommand()
        $migrationCmd.CommandText = @"
INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
VALUES ('20260122010830_AddDurationToPlaylistItem', '9.0.0');
"@
        $migrationCmd.ExecuteNonQuery() | Out-Null
        Write-Host "Migration history updated!"
        
    } finally {
        $connection.Close()
    }
} else {
    Write-Host "Database not found at $dbPath"
}
