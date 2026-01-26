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
}

public class SettingService : ISettingService
{
    private readonly IPlaylistDbContextFactory _dbContextFactory;

    private const string SelectedPlaylistIdKey = "SelectedPlaylistId";
    private const string FullscreenBehaviorKey = "FullscreenBehavior";
    private const string RunOnStartupKey = "RunOnStartup";

    public SettingService()
    {
        // Get the DbContextFactory from the application's service provider
        var app = (App)Application.Current;
        _dbContextFactory = app.ServiceProvider.GetRequiredService<IPlaylistDbContextFactory>();
    }
    
    public string GetSelectedPlaylistId()
    {
        var context = _dbContextFactory.CreateDbContext();
        var setting = context.Settings.Find(SelectedPlaylistIdKey);
        return setting?.Value ?? string.Empty;
    }

    public void SetSelectedPlaylistId(string playlistId)
    {
        var context = _dbContextFactory.CreateDbContext();
        var setting = context.Settings.Find(SelectedPlaylistIdKey);
        if (setting == null)
        {
            setting = new Models.Setting { Key = SelectedPlaylistIdKey, Value = playlistId };
            context.Settings.Add(setting);
        }
        else
        {
            setting.Value = playlistId;
        }
        context.SaveChanges();
    }

    public string GetFullscreenBehavior()
    {
        var context = _dbContextFactory.CreateDbContext();
        var setting = context.Settings.Find(FullscreenBehaviorKey);
        return setting?.Value ?? "Auto";
    }

    public void SetFullscreenBehavior(string behavior)
    {
        var context = _dbContextFactory.CreateDbContext();
        var setting = context.Settings.Find(FullscreenBehaviorKey);
        if (setting == null)
        {
            setting = new Models.Setting { Key = FullscreenBehaviorKey, Value = behavior };
            context.Settings.Add(setting);
        }
        else
        {
            setting.Value = behavior;
        }
        context.SaveChanges();
    }

    public bool GetRunOnStartup()
    {
        var context = _dbContextFactory.CreateDbContext();
        var setting = context.Settings.Find(RunOnStartupKey);
        var value = setting?.Value;
        return bool.TryParse(value, out var enabled) && enabled;
    }

    public void SetRunOnStartup(bool enabled)
    {
        var context = _dbContextFactory.CreateDbContext();
        var setting = context.Settings.Find(RunOnStartupKey);
        if (setting == null)
        {
            setting = new Models.Setting { Key = RunOnStartupKey, Value = enabled.ToString() };
            context.Settings.Add(setting);
        }
        else
        {
            setting.Value = enabled.ToString();
        }
        context.SaveChanges();
    }
}