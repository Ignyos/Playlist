using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Playlist.Data;
using Playlist.Services;

namespace Playlist.Views;

public partial class HistoryWindow : Window
{
    public class HistoryEntry
    {
        public DateTime TimeStamp { get; set; }
        public string PlaylistName { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
    }

    private readonly IPlaylistDbContextFactory _dbContextFactory;

    public HistoryWindow()
    {
        InitializeComponent();
        
        // Get the DbContextFactory from the application's service provider
        var app = (App)Application.Current;
        _dbContextFactory = app.ServiceProvider.GetRequiredService<IPlaylistDbContextFactory>();
        
        LoadHistory();
    }

    private void LoadHistory()
    {
        try
        {
            var context = _dbContextFactory.CreateDbContext();
            
            var historyEntries = context.History
                .Include(h => h.Playlist)
                .Include(h => h.PlaylistItem)
                .OrderByDescending(h => h.TimeStamp)
                .Select(h => new HistoryEntry
                {
                    TimeStamp = h.TimeStamp,
                    PlaylistName = h.Playlist.Name,
                    ItemName = h.PlaylistItem.Name
                })
                .Take(1000) // Limit to last 1000 entries
                .ToList();

            HistoryDataGrid.ItemsSource = historyEntries;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading history: {ex.Message}", "Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}


