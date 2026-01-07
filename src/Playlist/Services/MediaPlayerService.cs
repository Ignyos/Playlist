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
        public bool IsPlaying => _mediaPlayer.IsPlaying;

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

                // If continuing, set the start time
                if (continueFromTimestamp && item.TimeStamp.HasValue)
                {
                    _mediaPlayer.Time = item.TimeStamp.Value * 1000; // Convert seconds to milliseconds
                }

                // Start playback
                _mediaPlayer.Play();

                // Update LastPlayed
                item.LastPlayed = DateTime.Now;
                var playlist = await _dbContext.Playlists
                    .FirstOrDefaultAsync(p => p.Id == item.PlaylistId);
                if (playlist != null)
                {
                    playlist.LastPlayed = DateTime.Now;
                }
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
            if (_currentItem != null && _mediaPlayer.IsPlaying)
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

                // Reset timestamp since playback completed
                var item = await _dbContext.PlaylistItems
                    .FirstOrDefaultAsync(i => i.Id == _currentItem.Id);
                if (item != null)
                {
                    item.TimeStamp = null;
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
            // Periodically save timestamp (every 10 seconds)
            if (_currentItem != null && e.Time % 10000 < 500)
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

            _mediaPlayer.Playing -= OnMediaPlaying;
            _mediaPlayer.EndReached -= OnMediaEnded;
            _mediaPlayer.EncounteredError -= OnMediaError;
            _mediaPlayer.TimeChanged -= OnTimeChanged;

            _mediaPlayer.Stop();
            _mediaPlayer.Dispose();
            _libVLC.Dispose();

            _disposed = true;
        }
    }
}
