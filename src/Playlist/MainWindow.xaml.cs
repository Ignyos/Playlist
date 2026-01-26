using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Playlist.Data;
using Playlist.Services;
using Playlist.ViewModels;
using Playlist.Views;

namespace Playlist;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IPlaylistDbContextFactory _dbContextFactory;
    private readonly ISettingService _settingService;
    private readonly ObservableCollection<PlaylistViewModel> _playlists;
    private readonly ObservableCollection<PlaylistItemViewModel> _playlistItems;
    private PlaylistViewModel? _selectedPlaylist;
    private string _currentSort = "Name_Asc"; // Default sort
    private Point _dragStartPoint;
    private ListBoxInsertionAdorner? _insertionAdorner;
    private int _dragTargetIndex = -1; // Track which item index we're dragging over
    private MediaPlayerService? _mediaPlayerService;
    private MediaPlayerWindow? _mediaPlayerWindow;

    public MainWindow()
    {
        InitializeComponent();
        
        // Get services from the application's service provider
        var app = (App)Application.Current;
        _dbContextFactory = app.ServiceProvider.GetRequiredService<IPlaylistDbContextFactory>();
        _settingService = app.ServiceProvider.GetRequiredService<ISettingService>();
        
        // Set title with version
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        Title = $"Playlist v{version?.Major}.{version?.Minor}.{version?.Build}";
        
        _playlists = new ObservableCollection<PlaylistViewModel>();
        _playlistItems = new ObservableCollection<PlaylistItemViewModel>();
        
        // Initialize database with proper migration handling
        var dbContext = _dbContextFactory.CreateDbContext();
        InitializeDatabase(dbContext, version);

        // Apply startup preference stored in settings
        ApplyRunOnStartupSetting();
        
        _mediaPlayerService = new MediaPlayerService(dbContext);
        _mediaPlayerService.MediaEnded += OnMediaEnded;
        
        PlaylistsListBox.ItemsSource = _playlists;
        PlaylistItemsListBox.ItemsSource = _playlistItems;
        
        PlaylistsListBox.SelectionChanged += PlaylistsListBox_SelectionChanged;
        PlaylistItemsListBox.SelectionChanged += PlaylistItemsListBox_SelectionChanged;
        SearchTextBox.TextChanged += SearchTextBox_TextChanged;
        
        LoadPlaylists();
        
        // Restore previously selected playlist
        RestoreSelectedPlaylist();
    }

    private void RestoreSelectedPlaylist()
    {
        try
        {
            var selectedIdStr = _settingService.GetSelectedPlaylistId();
            
            if (!string.IsNullOrEmpty(selectedIdStr) && int.TryParse(selectedIdStr, out int selectedId))
            {
                var playlist = _playlists.FirstOrDefault(p => p.Id == selectedId);
                if (playlist != null)
                {
                    PlaylistsListBox.SelectedItem = playlist;
                }
            }
        }
        catch
        {
            // Ignore errors restoring selection
        }
    }

    private void ApplyRunOnStartupSetting()
    {
        try
        {
            var enableStartup = _settingService.GetRunOnStartup();

            StartupService.ApplyRunOnStartup(enableStartup);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to apply startup setting: {ex}");
        }
    }

    private void LoadPlaylists(string? searchTerm = null)
    {
        var context = _dbContextFactory.CreateDbContext();
        var service = new PlaylistService(context);
        var playlists = service.GetAllPlaylists(searchTerm);
        
        // Apply sorting
        IEnumerable<Models.Playlist> sortedPlaylists = _currentSort switch
        {
            "Name_Asc" => playlists.OrderBy(p => p.Name),
            "Name_Desc" => playlists.OrderByDescending(p => p.Name),
            "Created_Asc" => playlists.OrderBy(p => p.Created),
            "Created_Desc" => playlists.OrderByDescending(p => p.Created),
            "Played_Asc" => playlists.OrderBy(p => p.LastPlayed),
            "Played_Desc" => playlists.OrderByDescending(p => p.LastPlayed),
            _ => playlists.OrderBy(p => p.Name)
        };
        
        _playlists.Clear();
        
        foreach (var playlist in sortedPlaylists)
        {
            _playlists.Add(new PlaylistViewModel
            {
                Id = playlist.Id,
                Name = playlist.Name,
                Created = playlist.Created,
                LastPlayed = playlist.LastPlayed
            });
        }
    }

    private void LoadPlaylistItems(int playlistId)
    {
        var context = _dbContextFactory.CreateDbContext();
        var service = new PlaylistService(context);
        var items = service.GetPlaylistItems(playlistId);
        var playlist = service.GetPlaylistById(playlistId);
        
        _playlistItems.Clear();
        
        foreach (var item in items)
        {
            _playlistItems.Add(new PlaylistItemViewModel
            {
                Id = item.Id,
                PlaylistId = item.PlaylistId,
                Ordinal = item.Ordinal,
                Name = item.Name,
                Path = item.Path,
                LastPlayed = item.LastPlayed,
                TimeStamp = item.TimeStamp,
                Duration = item.Duration
            });
        }
        
        // Restore selected item for this playlist
        if (playlist?.SelectedItemId != null)
        {
            var selectedItem = _playlistItems.FirstOrDefault(i => i.Id == playlist.SelectedItemId);
            if (selectedItem != null)
            {
                PlaylistItemsListBox.SelectedItem = selectedItem;
            }
        }
    }

    private void PlaylistsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedPlaylist = PlaylistsListBox.SelectedItem as PlaylistViewModel;
        
        if (_selectedPlaylist != null)
        {
            LoadPlaylistItems(_selectedPlaylist.Id);
            
            // Save selected playlist ID to settings
            _settingService.SetSelectedPlaylistId(_selectedPlaylist.Id.ToString());
        }
        else
        {
            _playlistItems.Clear();
            
            // Clear selected playlist ID from settings
            _settingService.SetSelectedPlaylistId("");
        }
    }

    private void PlaylistItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_selectedPlaylist != null)
        {
            var selectedItem = PlaylistItemsListBox.SelectedItem as PlaylistItemViewModel;
            var context = _dbContextFactory.CreateDbContext();
            var service = new PlaylistService(context);
            service.UpdatePlaylistSelectedItem(_selectedPlaylist.Id, selectedItem?.Id);
        }
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadPlaylists(SearchTextBox.Text);
    }

    private void SortMenu_Click(object sender, RoutedEventArgs e)
    {
        SortContextMenu.PlacementTarget = SortMenuButton;
        SortContextMenu.IsOpen = true;
    }

    private void SortByCreatedDesc_Click(object sender, RoutedEventArgs e)
    {
        _currentSort = "Created_Desc";
        LoadPlaylists(SearchTextBox.Text);
    }

    private void SortByCreatedAsc_Click(object sender, RoutedEventArgs e)
    {
        _currentSort = "Created_Asc";
        LoadPlaylists(SearchTextBox.Text);
    }

    private void SortByPlayedDesc_Click(object sender, RoutedEventArgs e)
    {
        _currentSort = "Played_Desc";
        LoadPlaylists(SearchTextBox.Text);
    }

    private void SortByPlayedAsc_Click(object sender, RoutedEventArgs e)
    {
        _currentSort = "Played_Asc";
        LoadPlaylists(SearchTextBox.Text);
    }

    private void SortByNameAsc_Click(object sender, RoutedEventArgs e)
    {
        _currentSort = "Name_Asc";
        LoadPlaylists(SearchTextBox.Text);
    }

    private void SortByNameDesc_Click(object sender, RoutedEventArgs e)
    {
        _currentSort = "Name_Desc";
        LoadPlaylists(SearchTextBox.Text);
    }

    // Drag and Drop for Playlists
    private void Playlist_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void Playlist_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            Point currentPosition = e.GetPosition(null);
            Vector diff = _dragStartPoint - currentPosition;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var listBox = sender as ListBox;
                if (listBox?.SelectedItem is PlaylistViewModel item)
                {
                    DataObject dragData = new DataObject(typeof(PlaylistViewModel), item);
                    DragDrop.DoDragDrop(listBox, dragData, DragDropEffects.Move);
                }
            }
        }
    }

    private void Playlist_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(PlaylistViewModel)))
        {
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void Playlist_Drop(object sender, DragEventArgs e)
    {
        // Playlist reordering would require an Ordinal field in Playlist table
        // Not implemented yet as per requirements
    }

    // Drag and Drop for Playlist Items
    private void PlaylistItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void PlaylistItem_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            Point currentPosition = e.GetPosition(null);
            Vector diff = _dragStartPoint - currentPosition;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var listBox = sender as ListBox;
                if (listBox?.SelectedItem is PlaylistItemViewModel item)
                {
                    DataObject dragData = new DataObject(typeof(PlaylistItemViewModel), item);
                    RemoveInsertionAdorner(); // Clean up when drag ends
                    DragDrop.DoDragDrop(listBox, dragData, DragDropEffects.Move);
                }
            }
        }
    }

    private void PlaylistItem_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(PlaylistItemViewModel)))
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                var position = e.GetPosition(listBox);
                int targetIndex = listBox.Items.Count; // Default to end of list
                
                // Find the item index at this position
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    var container = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                    if (container != null && container.IsVisible)
                    {
                        var containerBounds = container.TransformToVisual(listBox).TransformBounds(new Rect(0, 0, container.ActualWidth, container.ActualHeight));
                        if (position.Y < containerBounds.Bottom)
                        {
                            targetIndex = i;
                            break;
                        }
                    }
                }
                
                _dragTargetIndex = targetIndex;
                ShowInsertionAdorner(listBox, targetIndex);
            }
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void PlaylistItem_Drop(object sender, DragEventArgs e)
    {
        RemoveInsertionAdorner();
        
        if (e.Data.GetDataPresent(typeof(PlaylistItemViewModel)) && _selectedPlaylist != null)
        {
            var droppedItem = e.Data.GetData(typeof(PlaylistItemViewModel)) as PlaylistItemViewModel;
            if (droppedItem == null) return;
            
            try
            {
                var listBox = sender as ListBox;
                if (listBox == null) return;
                
                var items = _playlistItems.ToList();
                var oldIndex = items.IndexOf(droppedItem);
                
                if (oldIndex == -1 || _dragTargetIndex < 0) return;
                
                // Clamp the target index to valid bounds
                int newIndex = Math.Min(_dragTargetIndex, items.Count);
                
                // Adjust new index if dragging backwards (removing from earlier position)
                if (oldIndex < newIndex)
                {
                    newIndex--;
                }
                
                // Only proceed if index actually changed
                if (oldIndex == newIndex) return;
                
                // Remove from old position and insert at new position
                items.RemoveAt(oldIndex);
                items.Insert(newIndex, droppedItem);
                
                // Update the database
                var itemIds = items.Select(i => i.Id).ToList();
                var context = _dbContextFactory.CreateDbContext();
                var service = new PlaylistService(context);
                service.ReorderPlaylistItems(_selectedPlaylist.Id, itemIds);
                
                // Reload to reflect changes
                LoadPlaylistItems(_selectedPlaylist.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reordering items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _dragTargetIndex = -1;
            }
        }
    }

    private void History_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var historyWindow = new HistoryWindow
            {
                Owner = this
            };
            historyWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening history: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsWindow = new SettingsWindow
            {
                Owner = this
            };

            settingsWindow.ShowDialog();
            ApplyRunOnStartupSetting();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void NewPlaylist_Click(object sender, RoutedEventArgs e)
    {
        var window = new NewPlaylistWindow();
        if (window.ShowDialog() == true || window.DialogResultValue)
        {
            try
            {
                var context = _dbContextFactory.CreateDbContext();
                var service = new PlaylistService(context);
                var newPlaylist = service.CreatePlaylist(window.PlaylistName, window.SelectedFiles.ToList());
                LoadPlaylists();
                
                // Select the newly created playlist
                var playlistToSelect = _playlists.FirstOrDefault(p => p.Id == newPlaylist.Id);
                if (playlistToSelect != null)
                {
                    PlaylistsListBox.SelectedItem = playlistToSelect;
                    
                    // Select the first item if there are any items
                    if (_playlistItems.Count > 0)
                    {
                        PlaylistItemsListBox.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                var errorDetails = ex.ToString();
                System.Diagnostics.Debug.WriteLine("=== Error creating playlist ===");
                System.Diagnostics.Debug.WriteLine(errorDetails);
                System.Diagnostics.Debug.WriteLine("================================");
                MessageBox.Show($"Error creating playlist: {ex.Message}\n\nSee debug output for full details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void EditPlaylist_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedPlaylist == null) return;

        var logFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Playlist", "debug.log");
        void Log(string message)
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logFile)!);
                File.AppendAllText(logFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}\n");
            }
            catch { }
        }

        // Window loads its own data using the playlist ID
        var window = new NewPlaylistWindow(_selectedPlaylist.Id);
        if (window.ShowDialog() == true || window.DialogResultValue)
        {
            try
            {
                var context = _dbContextFactory.CreateDbContext();
                var service = new PlaylistService(context);
                
                Log($"[EditPlaylist] Starting smart merge for playlist ID: {_selectedPlaylist.Id}");
                
                // Update playlist name
                Log($"[EditPlaylist] Updating name to: {window.PlaylistName}");
                service.UpdatePlaylist(_selectedPlaylist.Id, window.PlaylistName);
                Log("[EditPlaylist] Name updated successfully");
                
                // Get current items
                Log("[EditPlaylist] Getting current items...");
                var currentItems = service.GetPlaylistItems(_selectedPlaylist.Id);
                Log($"[EditPlaylist] Found {currentItems?.Count ?? 0} current items");
                
                if (currentItems == null)
                {
                    throw new Exception("GetPlaylistItems returned null");
                }
                
                // Smart merge: Compare paths
                var newPaths = window.SelectedFiles.ToHashSet();
                var existingPaths = currentItems.ToDictionary(i => i.Path, i => i);
                
                Log($"[EditPlaylist] New paths count: {newPaths.Count}");
                
                // Remove items that are no longer in the list
                Log("[EditPlaylist] Removing items not in new list...");
                var itemsToRemove = currentItems.Where(i => !newPaths.Contains(i.Path)).ToList();
                foreach (var item in itemsToRemove)
                {
                    Log($"[EditPlaylist] Removing: {item.Name} (path not in new list)");
                    service.RemovePlaylistItem(item.Id);
                }
                Log($"[EditPlaylist] Removed {itemsToRemove.Count} items");
                
                // Add new items that weren't previously in the playlist
                Log("[EditPlaylist] Adding new items...");
                var pathsToAdd = newPaths.Where(p => !existingPaths.ContainsKey(p)).ToList();
                if (pathsToAdd.Any())
                {
                    Log($"[EditPlaylist] Adding {pathsToAdd.Count} new files");
                    service.AddItemsToPlaylist(_selectedPlaylist.Id, pathsToAdd);
                }
                else
                {
                    Log("[EditPlaylist] No new items to add");
                }
                
                // Items that exist in both lists are preserved automatically (no action needed)
                var preservedCount = newPaths.Count(p => existingPaths.ContainsKey(p));
                Log($"[EditPlaylist] Preserved {preservedCount} existing items");
                
                // Reload UI - save the selected ID first!
                Log("[EditPlaylist] Reloading UI...");
                var selectedId = _selectedPlaylist.Id;
                LoadPlaylists();
                
                // Re-select the edited playlist
                var editedPlaylist = _playlists.FirstOrDefault(p => p.Id == selectedId);
                if (editedPlaylist != null)
                {
                    PlaylistsListBox.SelectedItem = editedPlaylist;
                }
                
                Log("[EditPlaylist] Smart merge complete!");
            }
            catch (Exception ex)
            {
                Log($"[EditPlaylist] ERROR: {ex.Message}");
                Log($"[EditPlaylist] Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error updating playlist: {ex.Message}\n\nStack trace:\n{ex.StackTrace}\n\nLog file: {logFile}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void RemovePlaylist_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedPlaylist == null)
        {
            MessageBox.Show("Please select a playlist to remove.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Are you sure you want to remove the playlist '{_selectedPlaylist.Name}'?",
            "Confirm Removal",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var context = _dbContextFactory.CreateDbContext();
                var service = new PlaylistService(context);
                service.DeletePlaylist(_selectedPlaylist.Id);
                LoadPlaylists();
                _playlistItems.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing playlist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void PlayItem_Click(object sender, RoutedEventArgs e)
    {
        var item = PlaylistItemsListBox.SelectedItem as PlaylistItemViewModel;
        if (item == null) return;

        PlayMedia(item, fromStart: true);
    }

    private void ContinueItem_Click(object sender, RoutedEventArgs e)
    {
        var item = PlaylistItemsListBox.SelectedItem as PlaylistItemViewModel;
        if (item == null) return;

        PlayMedia(item, fromStart: false);
    }

    private void PlaylistItem_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var item = PlaylistItemsListBox.SelectedItem as PlaylistItemViewModel;
        if (item == null) return;

        // If there's a saved timestamp, check if it's at 100% (completed)
        var context = _dbContextFactory.CreateDbContext();
        var dbItem = context.PlaylistItems.FirstOrDefault(i => i.Id == item.Id);
        
        bool isAtCompletion = false;
        if (dbItem?.TimeStamp != null && dbItem.TimeStamp > 0 && dbItem.Duration.HasValue && dbItem.Duration > 0)
        {
            var timeStampMs = dbItem.TimeStamp.Value * 1000L;
            int progress = (int)((timeStampMs * 100) / dbItem.Duration.Value);
            isAtCompletion = progress >= 100;
        }

        // Start from beginning if at completion, otherwise continue from timestamp
        PlayMedia(item, fromStart: isAtCompletion || (dbItem?.TimeStamp == null || dbItem.TimeStamp == 0));
    }

    private async void PlayMedia(PlaylistItemViewModel item, bool fromStart)
    {
        try
        {
            if (!System.IO.File.Exists(item.Path))
            {
                MessageBox.Show($"File not found: {item.Path}", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Always create a new window instance (recreates after disposal)
            _mediaPlayerWindow = new MediaPlayerWindow(_mediaPlayerService!);
            _mediaPlayerWindow.Closed += MediaPlayerWindow_Closed;
            _mediaPlayerWindow.Show();
            _mediaPlayerWindow.Activate();

            // Get the actual PlaylistItem from database
            var context = _dbContextFactory.CreateDbContext();
            var playlistItem = context.PlaylistItems.FirstOrDefault(i => i.Id == item.Id);
            if (playlistItem == null) return;

            // Apply fullscreen preference
            var fullscreenBehavior = _settingService.GetFullscreenBehavior();
            var enterFullscreen = fullscreenBehavior == "Auto";

            if (enterFullscreen)
            {
                await Dispatcher.InvokeAsync(() => _mediaPlayerWindow.EnterFullScreen());
            }

            // Play media with MediaPlayerService
            await _mediaPlayerService!.PlayAsync(playlistItem, continueFromTimestamp: !fromStart);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error playing media: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RenameItem_Click(object sender, RoutedEventArgs e)
    {
        var item = PlaylistItemsListBox.SelectedItem as PlaylistItemViewModel;
        if (item == null) return;

        var dialog = new Window
        {
            Title = "Rename Item",
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this
        };

        var stackPanel = new StackPanel { Margin = new Thickness(10) };
        var label = new Label { Content = "New Name:" };
        var textBox = new TextBox { Text = item.Name, Margin = new Thickness(0, 5, 0, 10) };
        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var okButton = new Button { Content = "OK", Width = 75, Margin = new Thickness(0, 0, 5, 0) };
        var cancelButton = new Button { Content = "Cancel", Width = 75 };

        okButton.Click += (s, args) => { dialog.DialogResult = true; dialog.Close(); };
        cancelButton.Click += (s, args) => { dialog.Close(); };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        stackPanel.Children.Add(label);
        stackPanel.Children.Add(textBox);
        stackPanel.Children.Add(buttonPanel);
        dialog.Content = stackPanel;

        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            try
            {
                var context = _dbContextFactory.CreateDbContext();
                var service = new PlaylistService(context);
                service.UpdatePlaylistItemName(item.Id, textBox.Text);
                
                // Reload the playlist items to reflect the change
                if (_selectedPlaylist != null)
                {
                    LoadPlaylistItems(_selectedPlaylist.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error renaming item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        var item = PlaylistItemsListBox.SelectedItem as PlaylistItemViewModel;
        if (item == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to remove '{item.Name}' from the playlist?",
            "Confirm Removal",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var context = _dbContextFactory.CreateDbContext();
                var service = new PlaylistService(context);
                service.RemovePlaylistItem(item.Id);
                if (_selectedPlaylist != null)
                {
                    LoadPlaylistItems(_selectedPlaylist.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void AboutPlaylist_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl("https://playlist.ignyos.com");
    }

    private void AboutIgnyos_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl("https://ignyos.com");
    }

    private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        var updateWindow = new Views.UpdateCheckWindow
        {
            Owner = this
        };
        updateWindow.ShowDialog();
    }

    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open URL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowInsertionAdorner(ListBox listBox, int targetIndex)
    {
        RemoveInsertionAdorner();
        
        var adornerLayer = AdornerLayer.GetAdornerLayer(listBox);
        if (adornerLayer != null)
        {
            _insertionAdorner = new ListBoxInsertionAdorner(listBox, targetIndex);
            adornerLayer.Add(_insertionAdorner);
        }
    }

    private void RemoveInsertionAdorner()
    {
        if (_insertionAdorner != null)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(_insertionAdorner.AdornedElement);
            adornerLayer?.Remove(_insertionAdorner);
            _insertionAdorner = null;
        }
    }

    // Helper method to find ancestor of specific type in visual tree
    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        do
        {
            if (current is T ancestor)
            {
                return ancestor;
            }
            current = VisualTreeHelper.GetParent(current);
        }
        while (current != null);
        return null;
    }

    private void OnMediaEnded(object? sender, Models.PlaylistItem e)
    {
        // Media playback completed, refresh the playlist to show updated progress
        Dispatcher.Invoke(() =>
        {
            if (_selectedPlaylist != null)
            {
                LoadPlaylistItems(_selectedPlaylist.Id);
            }
        });
    }

    private void MediaPlayerWindow_Closed(object? sender, EventArgs e)
    {
        // Media player window closed, refresh the playlist to show updated progress
        if (_selectedPlaylist != null)
        {
            LoadPlaylistItems(_selectedPlaylist.Id);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Clean up media player resources
        _mediaPlayerService?.Dispose();
        _mediaPlayerWindow?.Close();
        base.OnClosed(e);
    }

    private void InitializeDatabase(PlaylistDbContext dbContext, Version? version)
    {
        System.Diagnostics.Debug.WriteLine("=== InitializeDatabase START ===");
        System.Diagnostics.Debug.WriteLine($"Version: {version?.Major}.{version?.Minor}.{version?.Build}");
        
        try
        {
            // For v1.1.0 only: recreate database to ensure clean schema
            // TODO: Remove this in v1.1.1 or later
            if (version?.Major == 1 && version?.Minor == 1 && version?.Build == 0)
            {
                var dbPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Playlist", "playlist.db");
                
                System.Diagnostics.Debug.WriteLine($"Database path: {dbPath}");
                System.Diagnostics.Debug.WriteLine($"File exists: {File.Exists(dbPath)}");
                
                if (File.Exists(dbPath))
                {
                    System.Diagnostics.Debug.WriteLine("Deleting existing database...");
                    dbContext.Database.EnsureDeleted();
                    System.Diagnostics.Debug.WriteLine("Database deleted");
                }
            }
            
            // Apply all pending migrations
            System.Diagnostics.Debug.WriteLine("Running migrations...");
            dbContext.Database.Migrate();
            System.Diagnostics.Debug.WriteLine("Migrations completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in InitializeDatabase: {ex}");
            
            var errorMessage = $"Database initialization failed: {ex.Message}\n\n" +
                             $"The application will attempt to recreate the database.\n" +
                             $"Any existing data will be lost.\n\n" +
                             $"Continue?";
            
            var result = MessageBox.Show(errorMessage, "Database Error", 
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    dbContext.Database.EnsureDeleted();
                    dbContext.Database.Migrate();
                }
                catch (Exception recreateEx)
                {
                    MessageBox.Show($"Failed to recreate database: {recreateEx.Message}\n\n" +
                                  $"Please delete the database file manually at:\n" +
                                  $"%LocalAppData%\\Playlist\\playlist.db",
                                  "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
        
        System.Diagnostics.Debug.WriteLine("=== InitializeDatabase END ===");
    }
}

// Adorner class for showing insertion point on ListBox
internal class ListBoxInsertionAdorner : Adorner
{
    private readonly ListBox _listBox;
    private readonly int _targetIndex;

    public ListBoxInsertionAdorner(ListBox listBox, int targetIndex) : base(listBox)
    {
        _listBox = listBox;
        _targetIndex = targetIndex;
        IsHitTestVisible = false;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        // Handle case where target index is at the end of list
        if (_targetIndex >= _listBox.Items.Count)
        {
            // Draw line at bottom of last item
            var lastIndex = _listBox.Items.Count - 1;
            var lastContainer = _listBox.ItemContainerGenerator.ContainerFromIndex(lastIndex) as ListBoxItem;
            if (lastContainer == null) return;
            
            var lastBounds = lastContainer.TransformToVisual(_listBox).TransformBounds(
                new Rect(0, 0, lastContainer.ActualWidth, lastContainer.ActualHeight));
            
            var pen = new Pen(Brushes.DodgerBlue, 2);
            var point1 = new Point(0, lastBounds.Bottom);
            var point2 = new Point(lastBounds.Width, lastBounds.Bottom);
            
            drawingContext.DrawLine(pen, point1, point2);
            drawingContext.DrawEllipse(Brushes.DodgerBlue, null, point1, 3, 3);
            drawingContext.DrawEllipse(Brushes.DodgerBlue, null, point2, 3, 3);
        }
        else
        {
            // Find the container at the target index
            var targetContainer = _listBox.ItemContainerGenerator.ContainerFromIndex(_targetIndex) as ListBoxItem;
            if (targetContainer == null) return;
            
            // Get the position of the target container relative to the adorner (ListBox)
            var targetBounds = targetContainer.TransformToVisual(_listBox).TransformBounds(
                new Rect(0, 0, targetContainer.ActualWidth, targetContainer.ActualHeight));
            
            // Draw line at the top of the target item
            var pen = new Pen(Brushes.DodgerBlue, 2);
            var point1 = new Point(0, targetBounds.Top);
            var point2 = new Point(targetBounds.Width, targetBounds.Top);
            
            drawingContext.DrawLine(pen, point1, point2);
            
            // Draw small circles at the ends
            drawingContext.DrawEllipse(Brushes.DodgerBlue, null, point1, 3, 3);
            drawingContext.DrawEllipse(Brushes.DodgerBlue, null, point2, 3, 3);
        }
    }
}



