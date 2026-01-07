using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using LibVLCSharp.Shared;
using Playlist.Services;

namespace Playlist.Views
{
    public partial class MediaPlayerWindow : Window
    {
        private readonly MediaPlayerService _mediaPlayerService;
        private readonly DispatcherTimer _updateTimer;
        private readonly DispatcherTimer _hideControlsTimer;
        private bool _isDraggingProgress;
        private bool _isFullScreen;
        private WindowState _previousWindowState;
        private WindowStyle _previousWindowStyle;
        private ResizeMode _previousResizeMode;
        private bool _controlsVisible = true;

        public MediaPlayerWindow(MediaPlayerService mediaPlayerService)
        {
            InitializeComponent();

            _mediaPlayerService = mediaPlayerService;

            // Set the media player to the VideoView
            VideoView.MediaPlayer = _mediaPlayerService.Player;

            // Subscribe to events
            _mediaPlayerService.MediaStarted += OnMediaStarted;
            _mediaPlayerService.MediaEnded += OnMediaEnded;
            _mediaPlayerService.ErrorOccurred += OnErrorOccurred;
            _mediaPlayerService.Player.Playing += OnPlaying;
            _mediaPlayerService.Player.Paused += OnPaused;

            // Create timer to update progress
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            // Create timer to auto-hide controls
            _hideControlsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _hideControlsTimer.Tick += HideControlsTimer_Tick;

            // Set initial volume
            _mediaPlayerService.SetVolume((int)VolumeSlider.Value);
            
            // Show controls initially (this will start the hide timer)
            ShowControls();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            ShowControls();
        }

        private void OnMediaStarted(object? sender, Models.PlaylistItem e)
        {
            Dispatcher.Invoke(() =>
            {
                NowPlayingText.Text = e.Name;
                Title = $"Media Player - {e.Name}";
                ShowControls(); // Start auto-hide when media starts
            });
        }

        private void OnMediaEnded(object? sender, Models.PlaylistItem e)
        {
            Dispatcher.Invoke(() =>
            {
                NowPlayingText.Text = "Media playback completed";
                PlayPauseIcon.Text = "▶";
            });
        }

        private void OnErrorOccurred(object? sender, string e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(e, "Playback Error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        private void OnPlaying(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayPauseIcon.Text = "⏸";
            });
        }

        private void OnPaused(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayPauseIcon.Text = "▶";
            });
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (_mediaPlayerService.Player == null || _isDraggingProgress) return;

            var length = _mediaPlayerService.Player.Length;
            var time = _mediaPlayerService.Player.Time;

            if (length > 0)
            {
                ProgressSlider.Maximum = length;
                ProgressSlider.Value = time;

                CurrentTimeText.Text = FormatTime(time);
                DurationText.Text = FormatTime(length);
            }
        }

        private string FormatTime(long milliseconds)
        {
            var time = TimeSpan.FromMilliseconds(milliseconds);
            return time.Hours > 0 
                ? time.ToString(@"hh\:mm\:ss") 
                : time.ToString(@"mm\:ss");
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayerService.IsPlaying)
            {
                _mediaPlayerService.Pause();
            }
            else
            {
                _mediaPlayerService.Resume();
            }
        }

        private async void Stop_Click(object sender, RoutedEventArgs e)
        {
            await _mediaPlayerService.StopAsync();
            NowPlayingText.Text = "No media playing";
            Title = "Media Player";
            
            // Stop auto-hide timer and keep controls visible when stopped
            _hideControlsTimer.Stop();
            ShowControlsWithoutTimer();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _mediaPlayerService?.SetVolume((int)e.NewValue);
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            ToggleFullScreen();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                ToggleFullScreen();
            }
            else if (e.Key == Key.Escape && _isFullScreen)
            {
                ToggleFullScreen();
            }
            
            // Show controls on any key press
            ShowControls();
        }

        private void HideControlsTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isDraggingProgress && ControlsOverlay.Visibility == Visibility.Visible && !ControlsOverlay.IsMouseOver)
            {
                HideControls();
            }
        }

        private void ShowControls()
        {
            Dispatcher.Invoke(() =>
            {
                if (!_controlsVisible)
                {
                    ControlsOverlay.Visibility = Visibility.Visible;
                    var fadeIn = new DoubleAnimation
                    {
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(200)
                    };
                    ControlsOverlay.BeginAnimation(OpacityProperty, fadeIn);
                    _controlsVisible = true;
                }
                
                Cursor = Cursors.Arrow;
                _hideControlsTimer.Stop();
                _hideControlsTimer.Start();
            });
        }

        private void ShowControlsWithoutTimer()
        {
            Dispatcher.Invoke(() =>
            {
                if (!_controlsVisible)
                {
                    ControlsOverlay.Visibility = Visibility.Visible;
                    var fadeIn = new DoubleAnimation
                    {
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(200)
                    };
                    ControlsOverlay.BeginAnimation(OpacityProperty, fadeIn);
                    _controlsVisible = true;
                }
                
                Cursor = Cursors.Arrow;
            });
        }

        private void HideControls()
        {
            Dispatcher.Invoke(() =>
            {
                if (_controlsVisible)
                {
                    var fadeOut = new DoubleAnimation
                    {
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(200)
                    };
                    fadeOut.Completed += (s, e) => ControlsOverlay.Visibility = Visibility.Collapsed;
                    ControlsOverlay.BeginAnimation(OpacityProperty, fadeOut);
                    _controlsVisible = false;
                    Cursor = Cursors.None;
                }
            });
        }

        private void ToggleFullScreen()
        {
            if (_isFullScreen)
            {
                // Exit full screen
                Topmost = false;
                WindowState = _previousWindowState;
                WindowStyle = _previousWindowStyle;
                ResizeMode = _previousResizeMode;
                _isFullScreen = false;
                FullScreenIcon.Text = "⛶"; // Full screen icon
            }
            else
            {
                // Enter full screen
                _previousWindowState = WindowState;
                _previousWindowStyle = WindowStyle;
                _previousResizeMode = ResizeMode;
                
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Topmost = true;
                WindowState = WindowState.Maximized;
                _isFullScreen = true;
                FullScreenIcon.Text = "⬚"; // Exit full screen icon
            }
            
            ShowControls();
        }

        private void ProgressSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isDraggingProgress = true;
            ShowControls();
        }

        private void ProgressSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (_mediaPlayerService.Player != null)
            {
                _mediaPlayerService.Player.Time = (long)ProgressSlider.Value;
            }
            _isDraggingProgress = false;
            ShowControls();
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Update current time text when dragging
            if (_isDraggingProgress)
            {
                CurrentTimeText.Text = FormatTime((long)e.NewValue);
            }
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            // Stop playback if playing
            if (_mediaPlayerService.IsPlaying)
            {
                await _mediaPlayerService.StopAsync();
            }

            // Stop timers
            _updateTimer?.Stop();
            _hideControlsTimer?.Stop();

            // Unsubscribe from events
            if (_mediaPlayerService != null)
            {
                _mediaPlayerService.MediaStarted -= OnMediaStarted;
                _mediaPlayerService.MediaEnded -= OnMediaEnded;
                _mediaPlayerService.ErrorOccurred -= OnErrorOccurred;
                _mediaPlayerService.Player.Playing -= OnPlaying;
                _mediaPlayerService.Player.Paused -= OnPaused;
            }

            // Dispose VideoView to close the ForegroundWindow overlay
            VideoView.Dispose();
            
            // Allow the window to actually close now
            // Don't call base.OnClosed as it will be called automatically
        }

        protected override void OnClosed(EventArgs e)
        {
            // Cleanup already done in Window_Closing
            base.OnClosed(e);
        }
    }
}
