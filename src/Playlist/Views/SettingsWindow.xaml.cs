using System;
using System.Windows;
using Playlist.Data;
using Playlist.Services;

namespace Playlist.Views;

public partial class SettingsWindow : Window
{
    private const string RunOnStartupSettingKey = "RunOnStartup";
    private const string FullscreenBehaviorSettingKey = "FullscreenBehavior";
    private bool _isLoadingSettings = false;

    public SettingsWindow()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        _isLoadingSettings = true;
        try
        {
            using var context = new PlaylistDbContext();
            var service = new PlaylistService(context);
            
            // Load run on startup setting
            var runOnStartup = service.GetSetting(RunOnStartupSettingKey);
            if (bool.TryParse(runOnStartup, out var enabled))
            {
                RunOnStartupCheckBox.IsChecked = enabled;
            }
            else
            {
                RunOnStartupCheckBox.IsChecked = StartupService.IsRunOnStartupEnabled();
            }

            // Load fullscreen behavior setting
            var fullscreenBehavior = service.GetSetting(FullscreenBehaviorSettingKey);
            if (fullscreenBehavior == "Auto")
            {
                FullscreenAutoRadio.IsChecked = true;
            }
            else
            {
                FullscreenDefaultRadio.IsChecked = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isLoadingSettings = false;
        }
    }

    private void RunOnStartup_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoadingSettings) return;
        
        var enableStartup = RunOnStartupCheckBox.IsChecked == true;
        
        try
        {
            using var context = new PlaylistDbContext();
            var service = new PlaylistService(context);
            service.SetSetting(RunOnStartupSettingKey, enableStartup.ToString());
            StartupService.ApplyRunOnStartup(enableStartup);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void FullscreenBehavior_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoadingSettings) return;

        try
        {
            var behavior = FullscreenAutoRadio.IsChecked == true ? "Auto" : "Default";
            using var context = new PlaylistDbContext();
            var service = new PlaylistService(context);
            service.SetSetting(FullscreenBehaviorSettingKey, behavior);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
