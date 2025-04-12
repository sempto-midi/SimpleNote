using System.Windows;
using NAudio.Wave;
using SimpleNote.Audio;
using SimpleNote.Pages;

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

        private void InitializeMixerChannels(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var channelControl = new MixerChannel(i + 1);
                MixerPanel.Children.Add(channelControl);
            }
        }

        private void LoadPianoRoll()
        {
            MainFrame.Navigate(new PianoRollPage(_audioEngine, _metronome));
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
            MainFrame.Navigate(new PianoRollPage(_audioEngine, _metronome));
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