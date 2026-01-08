using System;
using System.Diagnostics;
using System.Windows;
using Playlist.Services;

namespace Playlist.Views
{
    public partial class UpdateCheckWindow : Window
    {
        private readonly UpdateService _updateService;
        private string _downloadUrl = string.Empty;

        public UpdateCheckWindow()
        {
            InitializeComponent();
            _updateService = new UpdateService();
            
            // Show current version immediately
            var currentVersion = _updateService.GetCurrentVersion();
            CurrentVersionText.Text = $"Current version: {currentVersion}";
            
            Loaded += UpdateCheckWindow_Loaded;
        }

        private async void UpdateCheckWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = await _updateService.CheckForUpdatesAsync();
                ShowResult(result);
            }
            catch (Exception ex)
            {
                ShowError($"Error checking for updates: {ex.Message}");
            }
        }

        private void ShowResult(UpdateCheckResult result)
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
            ResultPanel.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                ShowError(result.ErrorMessage);
                return;
            }

            if (result.IsUpdateAvailable)
            {
                HeaderText.Text = "Update Available!";
                StatusMessage.Text = $"A new version of Playlist is available!";
                VersionInfo.Text = $"Current version: {result.CurrentVersion}\nLatest version: {result.LatestVersion}";
                
                _downloadUrl = "https://playlist.ignyos.com";
                DownloadButton.Visibility = Visibility.Visible;
            }
            else
            {
                HeaderText.Text = "You're Up to Date!";
                StatusMessage.Text = "You are running the latest version of Playlist.";
                VersionInfo.Text = $"Current version: {result.CurrentVersion}";
            }
        }

        private void ShowError(string errorMessage)
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
            ResultPanel.Visibility = Visibility.Visible;
            
            HeaderText.Text = "Unable to Check";
            StatusMessage.Text = errorMessage;
            
            var currentVersion = _updateService.GetCurrentVersion();
            VersionInfo.Text = $"Current version: {currentVersion}";
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _downloadUrl,
                    UseShellExecute = true
                });
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open browser: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
