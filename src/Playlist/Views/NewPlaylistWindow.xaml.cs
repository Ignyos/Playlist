using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Microsoft.Extensions.DependencyInjection;
using Playlist.Data;
using Playlist.Services;

namespace Playlist.Views;

public partial class NewPlaylistWindow : Window
{
    public ObservableCollection<string> SelectedFiles { get; } = new();
    public string PlaylistName => PlaylistNameTextBox.Text;
    public bool DialogResultValue { get; private set; }
    private readonly IPlaylistDbContextFactory _dbContextFactory;

    public NewPlaylistWindow()
    {
        InitializeComponent();
        
        // Get the DbContextFactory from the application's service provider
        var app = (App)Application.Current;
        _dbContextFactory = app.ServiceProvider.GetRequiredService<IPlaylistDbContextFactory>();
        
        FilesListBox.ItemsSource = SelectedFiles;
    }

    public NewPlaylistWindow(int playlistId) : this()
    {
        // Load playlist data using own DbContext
        var context = _dbContextFactory.CreateDbContext();
        var service = new PlaylistService(context);
        var playlist = service.GetPlaylistById(playlistId);
        
        if (playlist != null)
        {
            PlaylistNameTextBox.Text = playlist.Name;
            var existingFiles = playlist.Items
                .Where(i => i.DeleteDate == null)
                .Select(i => i.Path)
                .ToList();
            
            foreach (var file in existingFiles)
            {
                SelectedFiles.Add(file);
            }
        }
        
        Title = "Edit Playlist";
    }

    private void AddFiles_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Media Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.mp3;*.wav;*.flac;*.aac;*.ogg;*.m4a|All Files|*.*",
            Title = "Select Media Files"
        };

        if (dialog.ShowDialog() == true)
        {
            foreach (var fileName in dialog.FileNames)
            {
                if (!SelectedFiles.Contains(fileName))
                {
                    SelectedFiles.Add(fileName);
                }
            }
        }
    }

    private void AddFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Folder with Media Files"
        };

        if (dialog.ShowDialog() == true)
        {
            var mediaExtensions = new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", 
                                         ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a" };
            
            var files = Directory.GetFiles(dialog.FolderName, "*.*", SearchOption.AllDirectories)
                .Where(f => mediaExtensions.Contains(Path.GetExtension(f).ToLower()))
                .OrderBy(f => f);

            foreach (var file in files)
            {
                if (!SelectedFiles.Contains(file))
                {
                    SelectedFiles.Add(file);
                }
            }
            
            // If playlist name is empty or default, suggest folder name
            if (string.IsNullOrWhiteSpace(PlaylistNameTextBox.Text) || 
                PlaylistNameTextBox.Text == "New Playlist")
            {
                var folderName = Path.GetFileName(dialog.FolderName);
                if (!string.IsNullOrWhiteSpace(folderName))
                {
                    PlaylistNameTextBox.Text = folderName;
                }
            }
        }
    }

    private void RemoveFiles_Click(object sender, RoutedEventArgs e)
    {
        var selectedItems = FilesListBox.SelectedItems.Cast<string>().ToList();
        foreach (var item in selectedItems)
        {
            SelectedFiles.Remove(item);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PlaylistNameTextBox.Text))
        {
            MessageBox.Show("Please enter a playlist name.", "Validation Error", 
                          MessageBoxButton.OK, MessageBoxImage.Warning);
            PlaylistNameTextBox.Focus();
            return;
        }

        if (SelectedFiles.Count == 0)
        {
            var result = MessageBox.Show("No files selected. Create empty playlist?", 
                                        "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
        }

        DialogResultValue = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResultValue = false;
        Close();
    }
}


