using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibVLCSharp.Shared;
using Playlist.Data;
using Playlist.Models;
using Microsoft.EntityFrameworkCore;

namespace Playlist.Services
{
    public class MediaPlayerService : IDisposable
    {
        private readonly LibVLC _libVLC;
        private readonly MediaPlayer _mediaPlayer;
        private readonly PlaylistDbContext _dbContext;
        private PlaylistItem? _currentItem;
        private DateTime? _playbackStartTime;
        private bool _disposed;

        public event EventHandler<PlaylistItem>? MediaStarted;
        public event EventHandler<PlaylistItem>? MediaEnded;
        public event EventHandler<string>? ErrorOccurred;

        public MediaPlayer Player => _mediaPlayer;
        public PlaylistItem? CurrentItem => _currentItem;
        public bool IsPlaying => !_disposed && _mediaPlayer.IsPlaying;

        public MediaPlayerService(PlaylistDbContext dbContext)
        {
            _dbContext = dbContext;
            
            // Initialize LibVLC
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);

            // Subscribe to events
            _mediaPlayer.Playing += OnMediaPlaying;
            _mediaPlayer.EndReached += OnMediaEnded;
            _mediaPlayer.EncounteredError += OnMediaError;
            _mediaPlayer.TimeChanged += OnTimeChanged;
        }

        public async Task PlayAsync(PlaylistItem item, bool continueFromTimestamp = false)
        {
            try
            {
                if (!File.Exists(item.Path))
                {
                    var error = $"File not found: {item.Path}";
                    await LogErrorAsync(item, error);
                    ErrorOccurred?.Invoke(this, error);
                    return;
                }

                // Stop current playback if any
                await StopAsync();

                _currentItem = item;
                _playbackStartTime = DateTime.Now;

                // Create media from file
                using var media = new Media(_libVLC, item.Path, FromType.FromPath);
                _mediaPlayer.Media = media;

                // Start playback
                _mediaPlayer.Play();

                // Wait for media to start loading and metadata to be parsed
                // Retry up to 10 times with increasing delays
                long duration = 0;
                for (int attempt = 0; attempt < 10; attempt++)
                {
                    await Task.Delay(100 * (attempt + 1)); // 100ms, 200ms, 300ms, etc.
                    duration = _mediaPlayer.Length;
                    if (duration > 0)
                        break;
                }
                
                // Capture duration if not already stored
                if ((!item.Duration.HasValue || item.Duration.Value == 0) && duration > 0)
                {
                    var dbItem = await _dbContext.PlaylistItems
                        .FirstOrDefaultAsync(i => i.Id == item.Id);
                    if (dbItem != null)
                    {
                        dbItem.Duration = duration;
                        await _dbContext.SaveChangesAsync();
                        item.Duration = duration; // Update the in-memory object too
                    }
                }

                // If continuing, set the start time after playback begins
                if (continueFromTimestamp && item.TimeStamp.HasValue)
                {
                    _mediaPlayer.Time = item.TimeStamp.Value * 1000; // Convert seconds to milliseconds
                }

                // Update LastPlayed
                item.LastPlayed = DateTime.Now;
                var playlist = await _dbContext.Playlists
                    .FirstOrDefaultAsync(p => p.Id == item.PlaylistId);
                if (playlist != null)
                {
                    playlist.LastPlayed = DateTime.Now;
                }
                
                // Add history record
                var history = new History
                {
                    PlaylistId = item.PlaylistId,
                    PlaylistItemId = item.Id,
                    TimeStamp = DateTime.Now
                };
                _dbContext.History.Add(history);
                
                await _dbContext.SaveChangesAsync();

                MediaStarted?.Invoke(this, item);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(item, ex.Message, ex.StackTrace);
                ErrorOccurred?.Invoke(this, ex.Message);
            }
        }

        public async Task StopAsync()
        {
            if (_disposed || _currentItem == null) return;
            
            if (_mediaPlayer.IsPlaying)
            {
                // Save current timestamp
                var currentTimeSeconds = (int)(_mediaPlayer.Time / 1000);
                
                _mediaPlayer.Stop();

                // Update timestamp in database
                var item = await _dbContext.PlaylistItems
                    .FirstOrDefaultAsync(i => i.Id == _currentItem.Id);
                if (item != null)
                {
                    item.TimeStamp = currentTimeSeconds;
                    await _dbContext.SaveChangesAsync();
                }

                _currentItem = null;
                _playbackStartTime = null;
            }
        }

        public void Pause()
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
            }
        }

        public void Resume()
        {
            if (_mediaPlayer.CanPause && !_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Play();
            }
        }

        public void SetVolume(int volume)
        {
            _mediaPlayer.Volume = Math.Clamp(volume, 0, 100);
        }

        private void OnMediaPlaying(object? sender, EventArgs e)
        {
            // Media has started playing
        }

        private async void OnMediaEnded(object? sender, EventArgs e)
        {
            if (_currentItem == null) return;

            try
            {
                // Log to history
                await LogHistoryAsync(_currentItem);

                // Keep timestamp at duration (100%) since playback completed
                var item = await _dbContext.PlaylistItems
                    .FirstOrDefaultAsync(i => i.Id == _currentItem.Id);
                if (item != null && item.Duration.HasValue && item.Duration.Value > 0)
                {
                    // Set timestamp to duration (in seconds) to represent 100% progress
                    // Use rounding to avoid losing precision from integer division
                    item.TimeStamp = (int)Math.Round(item.Duration.Value / 1000.0);
                    await _dbContext.SaveChangesAsync();
                }

                var endedItem = _currentItem;
                _currentItem = null;
                _playbackStartTime = null;

                MediaEnded?.Invoke(this, endedItem);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(_currentItem, ex.Message, ex.StackTrace);
            }
        }

        private async void OnMediaError(object? sender, EventArgs e)
        {
            if (_currentItem != null)
            {
                var errorMsg = "VLC Media Player encountered an error during playback";
                await LogErrorAsync(_currentItem, errorMsg);
                ErrorOccurred?.Invoke(this, errorMsg);
            }
        }

        private async void OnTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
        {
            // Periodically save timestamp (every 1 second)
            if (_currentItem != null && e.Time % 1000 < 500)
            {
                var currentTimeSeconds = (int)(e.Time / 1000);
                var item = await _dbContext.PlaylistItems
                    .FirstOrDefaultAsync(i => i.Id == _currentItem.Id);
                if (item != null)
                {
                    item.TimeStamp = currentTimeSeconds;
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        private async Task LogHistoryAsync(PlaylistItem item)
        {
            try
            {
                var history = new History
                {
                    PlaylistId = item.PlaylistId,
                    PlaylistItemId = item.Id,
                    TimeStamp = _playbackStartTime ?? DateTime.Now
                };

                _dbContext.History.Add(history);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error but don't throw
                await LogErrorAsync(item, $"Failed to log history: {ex.Message}", ex.StackTrace);
            }
        }

        private async Task LogErrorAsync(PlaylistItem? item, string errorMessage, string? stackTrace = null)
        {
            try
            {
                var errorLog = new ErrorLog
                {
                    PlaylistId = item?.PlaylistId,
                    PlaylistItemId = item?.Id,
                    TimeStamp = DateTime.Now,
                    ErrorMessage = errorMessage,
                    StackTrace = stackTrace ?? string.Empty
                };

                _dbContext.ErrorLogs.Add(errorLog);
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                // If we can't log the error, there's nothing more we can do
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true; // Set this first to prevent any further access

            _mediaPlayer.Playing -= OnMediaPlaying;
            _mediaPlayer.EndReached -= OnMediaEnded;
            _mediaPlayer.EncounteredError -= OnMediaError;
            _mediaPlayer.TimeChanged -= OnTimeChanged;

            try
            {
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Stop();
                }
            }
            catch
            {
                // Ignore errors during disposal
            }

            _mediaPlayer.Dispose();
            _libVLC.Dispose();
        }
    }
}
