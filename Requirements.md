# Playlist
## Windows application designed to manage playlists of videos and audio

## UI Layout

### Main Window

- A top row of menu options: File, About
- A left-hand side panel with lists of playlists
- A main panel that displays the items in the selected list

#### File menu options

- New Playlist
  - Opens a new window
    - Input for name of playlist (default "New Playlist")
    - File/folder picker to select files to play
    - List of currently selected files
    - Cancel & Save buttons
- History
  - Opens a new window with a list of what has been played
- Settings
  - Opens a new window with settings options TBD
- Exit
  - Closes the application

#### About menu options

- Playlist
  - Opens in browser https://playlist.ignyos.com
- Ignyos
  - Opens in browser https://ignyos.com

### Left-hand side panel

- Search input at top that searches names of playlists
- Menu icon (hamburger) to left of search input
  - Sort by create date
    - Newest first
    - Oldest first
  - Sort by last played
    - Most recent first
    - Longest ago first
  - Sort by name
    - A - Z (default)
    - Z - A
- List of playlist items
  - Right click for context menu:
    - Edit: opens the same window as New Playlist option from File menu
      - When editing, the system uses a smart merge approach:
        - Existing items whose paths are still in the updated file list are preserved (keeps custom names, play history, timestamps, ordinal position)
        - Items whose paths are no longer in the updated file list are removed (soft delete via DeleteDate)
        - New items whose paths weren't previously in the playlist are added
      - This preserves user customizations (renamed items, play counts, timestamps) when editing playlists
    - Remove: confirmation window (yes/no). Yes marks the item's DeleteDate in the db
  - Left click to select
  - Left click and hold to reorder items in the list

### Main panel

- A banner at the top that displays the name of the Playlist being viewed
- A list of video or audio files in the playlist
  - Right click for context menu:
    - Play: plays the current file from start
    - Continue: plays the current file from the timestamp where it was last stopped
    - Rename: allows rename of 'name' in the list, does not affect the file
    - Remove: remove file from list. (marks item's DeletedDate)
  - Left click to select (does not automatically play)
  - Left click and hold to reorder items
  - Double left click to play

### VLC Media Player integration

- Ideal option:
  - There is another window that is a wrapper around the player that can control what is being played. This offers the ability to stop a media file playback and start another one when "Play" is selected from the Main panel list item or by double clicking it. It also enables us to control the sizing of the player separate from the main page of the application.
- Next best option:
  - VLC media player is launched and controled separately from the application. It would still be prefered if the main application can stop the current media and start a new one from the controls of the application.
- Minimum requirement:
  - VLC is required by the application and not embeded. Clicking play launches it and starts playback of the media. Less ideal because no control of what is currently playing.

### Data Model

- Setting
  - Key (string) (unique)
  - Value (string) - could be json or whatever is needed for that setting
- Playlist
  - Id (int) (unique)
  - Name (string)
  - Created (DateTime)
  - LastPlayed (DateTime)
  - DeleteDate (DateTime) (nullable) null if not deleted, the deletion date otherwise
  - SelectedItemId (int) (nullable) the id of the selected item, if any
- PlaylistItem
  - Id (int) (unique)
  - PlaylistId (int)
  - Ordinal (int) order in playlist (if not sorted by name or last playback)
  - Path (string) path to the file
  - Name (string) (default to path)
  - LastPlayed (DateTime)
  - TimeStamp (int) (nullable) the number of seconds at which it was stopped
  - DeleteDate (DateTime) (nullable) null if not deleted, the deletion date otherwise
- History
  - Id (int) (unique)
  - PlaylistId (int)
  - PlaylistItemId (int)
  - TimeStamp (DateTime) the date and time playback of this file was started
- ErrorLog
  - Id (int) (unique)
  - PlaylistId (int) (nullable)
  - PlaylistItemId (int) (nullable)
  - TimeStamp (DateTime) the time the error happened
  - ErrorMessage (string)
  - StackTrace (string)

## Tech Structure

- WPF Application
- Embedded VLC Media Player
- SQLite database to store data
- InnoSetup for building installers
- Playlist/doc for GitHub pages
  - Use GitHub Actions Release workflow to manage releases
  - Use GitHub releases to host downloads

## Non-functional requirements

- Performance expectations: TBD
- Target Windows 10/11
- Error handling
  - All errors are logged in internal SQLite db. If the error happens during playback, log the id of the Playlist and PlaylistItem