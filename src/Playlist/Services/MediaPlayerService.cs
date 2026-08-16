using System;
using System.IO;
using System.Linq;
using System.Threading;
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
        private readonly IPlaylistDbContextFactory _dbContextFactory;
        private readonly SemaphoreSlim _dbLock = new SemaphoreSlim(1, 1);
        private PlaylistItem? _currentItem;
        private DateTime _lastTimestampSave = DateTime.MinValue;
        private static readonly TimeSpan TimestampSaveInterval = TimeSpan.FromMilliseconds(250);
        private bool _isStopping;
        private bool _disposed;

        public event EventHandler<PlaylistItem>? MediaStarted;
        public event EventHandler<PlaylistItem>? MediaEnded;
        public event EventHandler<string>? ErrorOccurred;

        public MediaPlayer Player => _mediaPlayer;
        public PlaylistItem? CurrentItem => _currentItem;
        public bool IsPlaying => !_disposed && _mediaPlayer.IsPlaying;

        public MediaPlayerService(IPlaylistDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            
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
                
                // Capture duration if not already stored, or if a prior synthetic completion
                // value (1000ms) is clearly wrong for the actual media length.
                if (ShouldRefreshStoredDuration(item.Duration, duration))
                {
                    await _dbLock.WaitAsync();
                    try
                    {
                        using var context = _dbContextFactory.CreateDbContext();
                        var dbItem = await context.PlaylistItems
                            .FirstOrDefaultAsync(i => i.Id == item.Id);
                        if (dbItem != null)
                        {
                            dbItem.Duration = duration;
                            await context.SaveChangesAsync();
                            item.Duration = duration; // Update the in-memory object too
                        }
                    }
                    finally
                    {
                        _dbLock.Release();
                    }
                }

                // If continuing, set the start time after playback begins
                if (continueFromTimestamp && item.TimeStamp.HasValue)
                {
                    _mediaPlayer.Time = item.TimeStamp.Value * 1000; // Convert seconds to milliseconds
                }

                await _dbLock.WaitAsync();
                try
                {
                    using var context = _dbContextFactory.CreateDbContext();

                    // Update LastPlayed
                    item.LastPlayed = DateTime.Now;
                    var playlist = await context.Playlists
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
                    context.History.Add(history);

                    await context.SaveChangesAsync();
                }
                finally
                {
                    _dbLock.Release();
                }

                MediaStarted?.Invoke(this, item);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(item, ex.Message, ex.StackTrace);
                ErrorOccurred?.Invoke(this, ex.Message);
            }
        }

        public async Task StopAsync(int? currentTimeSeconds = null)
        {
            if (_disposed || _currentItem == null) return;
            
            if (_mediaPlayer.IsPlaying)
            {
                _isStopping = true;
                var stoppingItem = _currentItem;
                _currentItem = null;

                // Prefer an explicit UI-provided timestamp, then the last known playback timestamp,
                // and only fall back to the live player clock as a last resort.
                var timestampSeconds = currentTimeSeconds
                    ?? stoppingItem?.TimeStamp
                    ?? (int)(_mediaPlayer.Time / 1000);

                _mediaPlayer.Stop();

                // Update timestamp in database
                await _dbLock.WaitAsync();
                try
                {
                    using var context = _dbContextFactory.CreateDbContext();
                    var item = await context.PlaylistItems
                        .FirstOrDefaultAsync(i => i.Id == stoppingItem!.Id);
                    if (item != null)
                    {
                        item.TimeStamp = timestampSeconds;
                        await context.SaveChangesAsync();
                    }
                }
                finally
                {
                    _dbLock.Release();
                    _isStopping = false;
                }
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
            if (_currentItem == null || _isStopping) return;

            try
            {
                // Keep timestamp at duration (100%) since playback completed
                await _dbLock.WaitAsync();
                try
                {
                    using var context = _dbContextFactory.CreateDbContext();
                    var item = await context.PlaylistItems
                        .FirstOrDefaultAsync(i => i.Id == _currentItem.Id);
                    if (item != null)
                    {
                        var durationMs = item.Duration.HasValue && item.Duration.Value > 0
                            ? item.Duration.Value
                            : _mediaPlayer.Length;

                        if (durationMs > 0)
                        {
                            // Ensure duration is saved if it was missing
                            if (!item.Duration.HasValue || item.Duration.Value == 0)
                            {
                                item.Duration = durationMs;
                                _currentItem.Duration = durationMs;
                            }

                            // Set timestamp to duration (in seconds) to represent 100% progress
                            // Use ceiling to avoid 99% due to truncation
                            var endSeconds = (int)Math.Ceiling(durationMs / 1000.0);
                            item.TimeStamp = endSeconds;
                            _currentItem.TimeStamp = endSeconds;
                            await context.SaveChangesAsync();
                        }
                    }
                }
                finally
                {
                    _dbLock.Release();
                }

                var endedItem = _currentItem;
                _currentItem = null;

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
            // Periodically save timestamp (every 250ms)
            // Use non-blocking tryacquire: if a save is already in progress, skip this tick
            if (_currentItem != null && !_isStopping && (DateTime.Now - _lastTimestampSave) >= TimestampSaveInterval)
            {
                if (!_dbLock.Wait(0)) return;
                try
                {
                    _lastTimestampSave = DateTime.Now;
                    using var context = _dbContextFactory.CreateDbContext();
                    var currentTimeSeconds = (int)(e.Time / 1000);
                    var item = await context.PlaylistItems
                        .FirstOrDefaultAsync(i => i.Id == _currentItem.Id);
                    if (item != null)
                    {
                        item.TimeStamp = currentTimeSeconds;
                        _currentItem.TimeStamp = currentTimeSeconds;
                        await context.SaveChangesAsync();
                    }
                }
                finally
                {
                    _dbLock.Release();
                }
            }
        }

        private async Task LogErrorAsync(PlaylistItem? item, string errorMessage, string? stackTrace = null)
        {
            try
            {
                await _dbLock.WaitAsync();
                try
                {
                    using var context = _dbContextFactory.CreateDbContext();
                    var errorLog = new ErrorLog
                    {
                        PlaylistId = item?.PlaylistId,
                        PlaylistItemId = item?.Id,
                        TimeStamp = DateTime.Now,
                        ErrorMessage = errorMessage,
                        StackTrace = stackTrace ?? string.Empty
                    };

                    context.ErrorLogs.Add(errorLog);
                    await context.SaveChangesAsync();
                }
                finally
                {
                    _dbLock.Release();
                }
            }
            catch
            {
                // If we can't log the error, there's nothing more we can do
            }
        }

        private static bool ShouldRefreshStoredDuration(long? storedDuration, long measuredDuration)
        {
            if (measuredDuration <= 0)
            {
                return false;
            }

            if (!storedDuration.HasValue || storedDuration.Value <= 0)
            {
                return true;
            }

            // Duration=1000 is used as a synthetic completion marker when no real duration
            // was known. Replace it as soon as we can measure the actual media length.
            return storedDuration.Value <= 1000 && measuredDuration > 1000;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true; // Set this first to prevent any further access
            _dbLock.Dispose();

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
