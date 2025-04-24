using ManagedBass;
using Microsoft.Win32;
using NAudio.Wave;
using SimpleNote.Audio;
using SimpleNote.Plugins;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleNote
{
    public partial class Workspace : Window
    {
        private readonly AudioEngine _audioEngine;
        private readonly Metronome _metronome;
        private readonly PluginManager _pluginManager;
        private int _projectId;
        private int _userId;

        public Workspace(int projectId, int userId)
        {
            InitializeComponent();
            _projectId = projectId;
            _userId = userId;

            _pluginManager = new PluginManager();
            _audioEngine = new AudioEngine(_pluginManager);
            _metronome = new Metronome(int.Parse(Tempo.Text));

            InitializePlugins();
            InitializeMixerChannels(9);
            LoadPianoRoll();
            InitializeChannelRack();
        }

        private void InitializeChannelRack()
        {
            AddPlugin.Click += (s, e) => ShowPluginSelectionDialog();
            UpdateChannelRackUI();
        }

        private void InitializePlugins()
        {
            // Инициализация Bass с настройками по умолчанию
            if (!Bass.Init())
            {
                MessageBox.Show("Audio engine initialization failed", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Загрузка плагинов из папки
            var pluginsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            if (Directory.Exists(pluginsDir))
            {
                foreach (var dll in Directory.GetFiles(pluginsDir, "*.dll"))
                {
                    try
                    {
                        _pluginManager.LoadVstPlugin(dll);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to load {Path.GetFileName(dll)}: {ex.Message}");
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Корректное освобождение ресурсов
            foreach (var plugin in _pluginManager.Plugins)
            {
                plugin.Dispose();
            }
            Bass.Free();

            base.OnClosed(e);
        }

        //private void ShowPluginSelectionDialog()
        //{
        //    var openFileDialog = new OpenFileDialog
        //    {
        //        Filter = "Plugin files (*.dll)|*.dll",
        //        InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"),
        //        Multiselect = true
        //    };

        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        foreach (var file in openFileDialog.FileNames)
        //        {
        //            try
        //            {
        //                var plugin = _pluginManager.LoadPlugin(file); // Исправлено на LoadPlugin
        //                plugin.ChannelId = _pluginManager.Plugins.Count + 1;
        //                _pluginManager.Plugins.Add(plugin);
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"Failed to load plugin: {ex.Message}", "Error",
        //                    MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }
        //        UpdateChannelRackUI();
        //    }
        //}

        private void UpdateChannelRackUI()
        {
            ChannelRack.Children.Clear();

            var title = new TextBlock
            {
                Text = "Channel Rack",
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.Orange,
                Margin = new Thickness(0, 0, 0, 10),
                FontSize = 16
            };
            ChannelRack.Children.Add(title);

            foreach (var plugin in _pluginManager.Plugins)
            {
                var pluginControl = CreatePluginControl(plugin);
                ChannelRack.Children.Add(pluginControl);
            }

            ChannelRack.Children.Add(AddPlugin);
        }

        private UIElement CreatePluginControl(IPlugin plugin)
        {
            var grid = new Grid
            {
                Width = 200,
                Height = 50,
                Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x2D)),
                Margin = new Thickness(0, 10, 0, 0)
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

            var nameText = new TextBlock
            {
                Text = $"{plugin.Name} (v{plugin.Version})",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0),
                Foreground = Brushes.White
            };
            Grid.SetColumn(nameText, 0);
            grid.Children.Add(nameText);

            var channelBox = new TextBox
            {
                Text = plugin.ChannelId.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0),
                Background = new SolidColorBrush(Color.FromRgb(0x3D, 0x3D, 0x3D)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                IsReadOnly = true
            };
            Grid.SetColumn(channelBox, 1);
            grid.Children.Add(channelBox);

            // Двойной клик для открытия редактора
            grid.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                {
                    ShowPluginEditor(plugin);
                }
            };

            return grid;
        }

        private void ShowPluginEditor(IPlugin plugin)
        {
            var editorWindow = new Window
            {
                Title = $"{plugin.Name} - Editor",
                Content = plugin.GetEditorView(),
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            editorWindow.ShowDialog();
        }

        private void LoadPianoRoll()
        {
            var pianoRoll = new PianoRoll(_audioEngine, _metronome, _pluginManager);
            pianoRoll.TimeUpdated += time => TimerDisplay.Text = time;
            MainFrame.Navigate(pianoRoll);
        }

        private void InitializeMixerChannels(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var channelControl = new MixerChannel(i + 1);
                MixerPanel.Children.Add(channelControl);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is PianoRoll pianoRoll)
            {
                pianoRoll.StartPlayback();
            }
            _metronome.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is PianoRoll pianoRoll)
            {
                pianoRoll.StopPlayback();
            }
            _metronome.Stop();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is PianoRoll pianoRoll)
            {
                pianoRoll.PausePlayback();
            }
            _metronome.Stop();
        }

        private void Metronome_Click(object sender, RoutedEventArgs e)
        {
            _metronome.Toggle();
        }

        private void ExportToMIDI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "MIDI Files (*.mid)|*.mid",
                    DefaultExt = ".mid",
                    FileName = GetDefaultProjectName()
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    if (MainFrame.Content is PianoRoll pianoRoll)
                    {
                        pianoRoll.ExportToMidi(saveFileDialog.FileName);
                    }

                    UpdateProjectInDatabase(saveFileDialog.FileName);

                    MessageBox.Show("Successfully exported to MIDI!", "Export",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetDefaultProjectName()
        {
            // Реализуйте получение имени проекта из БД
            return "New Project";
        }

        private void UpdateProjectInDatabase(string filePath)
        {
            // Реализуйте обновление проекта в БД
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExportToMP3_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}