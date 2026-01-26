using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Playlist.Services;

namespace Playlist.Views;

public partial class SettingsWindow : Window
{
    private bool _isLoadingSettings = false;
    private readonly ISettingService _settingService;

    public SettingsWindow()
    {
        InitializeComponent();
        
        // Get the SettingService from the application's service provider
        var app = (App)Application.Current;
        _settingService = app.ServiceProvider.GetRequiredService<ISettingService>();
        
        LoadSettings();
    }

    private void LoadSettings()
    {
        _isLoadingSettings = true;
        try
        {
            // Load run on startup setting
            var runOnStartup = _settingService.GetRunOnStartup();
            if (!runOnStartup)
            {
                RunOnStartupCheckBox.IsChecked = StartupService.IsRunOnStartupEnabled();
            }
            else
            {
                RunOnStartupCheckBox.IsChecked = true;
            }

            // Load fullscreen behavior setting
            var fullscreenBehavior = _settingService.GetFullscreenBehavior();
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
            _settingService.SetRunOnStartup(enableStartup);
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
            _settingService.SetFullscreenBehavior(behavior);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}



