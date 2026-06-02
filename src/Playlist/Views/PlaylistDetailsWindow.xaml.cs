using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Playlist.Data;
using Playlist.Models;
using Playlist.Services;

namespace Playlist.Views;

public partial class PlaylistDetailsWindow : Window
{
    private readonly int _playlistId;
    private readonly IPlaylistDbContextFactory _dbContextFactory;
    private bool _isLoading;

    private sealed class ModeOption
    {
        public PlaylistPlaybackMode Mode { get; init; }
        public string Label { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }

    private readonly List<ModeOption> _options =
    [
        new ModeOption
        {
            Mode = PlaylistPlaybackMode.StopAfterCurrent,
            Label = "Stop after current item",
            Description = "Playback stops when the current item finishes."
        },
        new ModeOption
        {
            Mode = PlaylistPlaybackMode.SequentialAutoNext,
            Label = "Auto-play next",
            Description = "Automatically plays the next item in playlist order."
        },
        new ModeOption
        {
            Mode = PlaylistPlaybackMode.SequentialAutoNextLoop,
            Label = "Auto-play next and loop",
            Description = "Automatically plays the next item and loops back to the first item after the last."
        },
        new ModeOption
        {
            Mode = PlaylistPlaybackMode.ShuffleContinuous,
            Label = "Shuffle continuous",
            Description = "Keeps playing random items and can repeat previously played items."
        },
        new ModeOption
        {
            Mode = PlaylistPlaybackMode.ShufflePlayOnce,
            Label = "Shuffle play-once",
            Description = "Plays items in random order without repeats until all items are played, then stops."
        }
    ];

    public PlaylistDetailsWindow(int playlistId)
    {
        InitializeComponent();

        _playlistId = playlistId;

        var app = (App)Application.Current;
        _dbContextFactory = app.ServiceProvider.GetRequiredService<IPlaylistDbContextFactory>();

        PlaybackModeComboBox.DisplayMemberPath = nameof(ModeOption.Label);
        PlaybackModeComboBox.ItemsSource = _options;

        LoadPlaylist();
    }

    private void LoadPlaylist()
    {
        _isLoading = true;
        SaveStatusText.Text = string.Empty;

        try
        {
            using var context = _dbContextFactory.CreateDbContext();
            var service = new PlaylistService(context);
            var playlist = service.GetPlaylistById(_playlistId);

            if (playlist == null)
            {
                MessageBox.Show("The selected playlist could not be found.", "Playlist Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            PlaylistNameText.Text = playlist.Name;

            var selectedOption = _options.Find(o => o.Mode == playlist.PlaybackMode)
                ?? _options[0];

            PlaybackModeComboBox.SelectedItem = selectedOption;
            ModeDescriptionText.Text = selectedOption.Description;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading playlist details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void PlaybackModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoading)
        {
            return;
        }

        if (PlaybackModeComboBox.SelectedItem is not ModeOption selectedOption)
        {
            return;
        }

        ModeDescriptionText.Text = selectedOption.Description;

        try
        {
            using var context = _dbContextFactory.CreateDbContext();
            var service = new PlaylistService(context);
            var updated = service.UpdatePlaylistPlaybackMode(_playlistId, selectedOption.Mode);

            if (!updated)
            {
                MessageBox.Show("The playlist could not be updated.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveStatusText.Text = $"Saved at {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving playback mode: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
