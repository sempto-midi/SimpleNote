using System.Windows;
using System.Windows.Input;
using NAudio.Wave;
using SimpleNote.Audio;

namespace SimpleNote
{
    public partial class Workspace : Window
    {
        private readonly AudioEngine _audioEngine;
        private Metronome _metronome;

        public Workspace()
        {
            InitializeComponent();
            _audioEngine = new AudioEngine();
            _metronome = new Metronome(int.Parse(Tempo.Text));

            // Инициализация интерфейса
            InitializeMixerChannels(8);
            LoadPianoRoll();
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

        private void InitializeMixerChannels(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var channelControl = new MixerChannel(i + 1);
                MixerPanel.Children.Add(channelControl);
            }
        }

        private void Mixer_Click(object sender, RoutedEventArgs e)
        {
            // Показываем/скрываем панель микшера
            MixerPanel.Visibility = MixerPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        //private void Plugins_Click(object sender, RoutedEventArgs e)
        //{
        //    Открываем меню или окно плагинов
        //   var pluginsWindow = new PluginsWindow();
        //    pluginsWindow.Owner = this;
        //    pluginsWindow.ShowDialog();
        //}

        private void LoadPianoRoll()
        {
            var pianoRoll = new PianoRoll(_audioEngine, _metronome);
            pianoRoll.TimeUpdated += time => TimerDisplay.Text = time;
            MainFrame.Navigate(pianoRoll);
        }

        // Обработчики кнопок
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            _metronome.Start();
            _audioEngine.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _metronome.Stop();
            _audioEngine.Stop();
            TimerDisplay.Text = "00:00:00";
        }

        private void PianoRoll_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PianoRoll(_audioEngine, _metronome));
        }

        private void Metronome_Click(object sender, RoutedEventArgs e)
        {
            _metronome.Toggle();
        }

        protected override void OnClosed(EventArgs e)
        {
            _audioEngine.Dispose();
            _metronome.Dispose();
            base.OnClosed(e);
        }
    }
}