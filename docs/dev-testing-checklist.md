# Development Testing Checklist

Use these VS Code launch profiles to run common test scenarios quickly.

## Launch Profiles
- Run Playlist (Normal Dev)
  - Purpose: Standard development run against current local DB state.
- Run Playlist (Scenario: Clean/New DB)
  - Purpose: Simulate first-run behavior with a fresh database.
  - Prep behavior: backs up existing DB, removes local DB, then builds and runs.
- Run Playlist (Scenario: Existing DB/Upgrade)
  - Purpose: Validate startup and migration behavior against an existing DB.
  - Prep behavior: backs up existing DB, then builds and runs.

## Database Locations
- Local DB path: %LOCALAPPDATA%\Playlist\playlist.db
- Backup folder: %LOCALAPPDATA%\Playlist\backups

## Scenario Passes
- Clean install/new DB
  - Use: Run Playlist (Scenario: Clean/New DB)
  - Verify: app launches, tables initialize, no initialization error, basic create/play flow works.
- Existing DB upgrade
  - Use: Run Playlist (Scenario: Existing DB/Upgrade)
  - Verify: app launches with existing data, migrations apply, no schema-related startup errors.
- Playback mode matrix
  - Use: Run Playlist (Normal Dev) or Existing DB/Upgrade
  - Verify each mode:
    - Stop after current item
    - Auto-play next
    - Auto-play next and loop
    - Shuffle continuous
    - Shuffle play-once
- Error-resilience checks
  - Verify: UI does not crash on media-ended transitions or mode changes.
  - Verify: startup recovers from missing schema elements where supported.

## Manual Notes
- Keep installed production build for baseline comparison.
- Close installed app before running dev scenarios that touch local DB state.
- If needed, restore from a backup DB file manually by copying it to %LOCALAPPDATA%\Playlist\playlist.db.
