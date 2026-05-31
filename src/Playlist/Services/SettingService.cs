using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Playlist.Data;

namespace Playlist.Services;

public interface ISettingService
{
    string GetSelectedPlaylistId();

    void SetSelectedPlaylistId(string playlistId);

    string GetFullscreenBehavior();

    void SetFullscreenBehavior(string behavior);

    bool GetRunOnStartup();

    void SetRunOnStartup(bool enabled);

    int GetPlaybackVolume();

    void SetPlaybackVolume(int volume);

    DateTime? GetLastUpdateCheckAttemptUtc();

    void SetLastUpdateCheckAttemptUtc(DateTime utcTimestamp);

    bool GetLastKnownUpdateAvailable();

    string GetLastKnownUpdateVersion();

    string GetLastKnownUpdateDownloadUrl();

    void SetLastKnownUpdateStatus(bool isAvailable, string latestVersion, string downloadUrl);
}

public class SettingService : ISettingService
{
    private readonly IPlaylistDbContextFactory _dbContextFactory;

    private const string SelectedPlaylistIdKey = "SelectedPlaylistId";
    private const string FullscreenBehaviorKey = "FullscreenBehavior";
    private const string RunOnStartupKey = "RunOnStartup";
    private const string PlaybackVolumeKey = "PlaybackVolume";
    private const string LastUpdateCheckAttemptUtcKey = "LastUpdateCheckAttemptUtc";
    private const string LastKnownUpdateAvailableKey = "LastKnownUpdateAvailable";
    private const string LastKnownUpdateVersionKey = "LastKnownUpdateVersion";
    private const string LastKnownUpdateDownloadUrlKey = "LastKnownUpdateDownloadUrl";

    public SettingService()
    {
        // Get the DbContextFactory from the application's service provider
        var app = (App)Application.Current;
        _dbContextFactory = app.ServiceProvider.GetRequiredService<IPlaylistDbContextFactory>();
    }
    
    public string GetSelectedPlaylistId()
    {
        return GetSettingValue(SelectedPlaylistIdKey, string.Empty);
    }

    public void SetSelectedPlaylistId(string playlistId)
    {
        SetSettingValue(SelectedPlaylistIdKey, playlistId);
    }

    public string GetFullscreenBehavior()
    {
        return GetSettingValue(FullscreenBehaviorKey, "Auto");
    }

    public void SetFullscreenBehavior(string behavior)
    {
        SetSettingValue(FullscreenBehaviorKey, behavior);
    }

    public bool GetRunOnStartup()
    {
        var value = GetSettingValue(RunOnStartupKey, bool.FalseString);
        return bool.TryParse(value, out var enabled) && enabled;
    }

    public void SetRunOnStartup(bool enabled)
    {
        SetSettingValue(RunOnStartupKey, enabled.ToString());
    }

    public int GetPlaybackVolume()
    {
        var rawValue = GetSettingValue(PlaybackVolumeKey, "80");
        if (!int.TryParse(rawValue, out var volume))
        {
            return 80;
        }

        return Math.Clamp(volume, 0, 100);
    }

    public void SetPlaybackVolume(int volume)
    {
        SetSettingValue(PlaybackVolumeKey, Math.Clamp(volume, 0, 100).ToString());
    }

    public DateTime? GetLastUpdateCheckAttemptUtc()
    {
        var rawValue = GetSettingValue(LastUpdateCheckAttemptUtcKey, string.Empty);
        if (DateTime.TryParse(rawValue, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
        {
            return parsed.ToUniversalTime();
        }

        return null;
    }

    public void SetLastUpdateCheckAttemptUtc(DateTime utcTimestamp)
    {
        SetSettingValue(LastUpdateCheckAttemptUtcKey, utcTimestamp.ToUniversalTime().ToString("O"));
    }

    public bool GetLastKnownUpdateAvailable()
    {
        var rawValue = GetSettingValue(LastKnownUpdateAvailableKey, bool.FalseString);
        return bool.TryParse(rawValue, out var isAvailable) && isAvailable;
    }

    public string GetLastKnownUpdateVersion()
    {
        return GetSettingValue(LastKnownUpdateVersionKey, string.Empty);
    }

    public string GetLastKnownUpdateDownloadUrl()
    {
        return GetSettingValue(LastKnownUpdateDownloadUrlKey, string.Empty);
    }

    public void SetLastKnownUpdateStatus(bool isAvailable, string latestVersion, string downloadUrl)
    {
        SetSettingValue(LastKnownUpdateAvailableKey, isAvailable.ToString());
        SetSettingValue(LastKnownUpdateVersionKey, isAvailable ? latestVersion : string.Empty);
        SetSettingValue(LastKnownUpdateDownloadUrlKey, isAvailable ? downloadUrl : string.Empty);
    }

    private string GetSettingValue(string key, string defaultValue)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var setting = context.Settings.Find(key);
        return setting?.Value ?? defaultValue;
    }

    private void SetSettingValue(string key, string value)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var setting = context.Settings.Find(key);
        if (setting == null)
        {
            setting = new Models.Setting { Key = key, Value = value };
            context.Settings.Add(setting);
        }
        else
        {
            setting.Value = value;
        }

        context.SaveChanges();
    }
}