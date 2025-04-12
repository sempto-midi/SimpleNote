using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Windows.Threading;

namespace SimpleNote.Audio
{
    public class Metronome
    {
        private readonly SignalGenerator _generator;
        private readonly WaveOutEvent _output;
        private readonly DispatcherTimer _timer;

        public Metronome(int bpm = 120)
        {
            _generator = new SignalGenerator(44100, 2)
            {
                Type = SignalGeneratorType.Square,
                Frequency = 1000,
                Gain = 0.2
            };

            _output = new WaveOutEvent();
            _output.Init(_generator.Take(TimeSpan.FromMilliseconds(50)));

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(60000.0 / bpm)
            };
            _timer.Tick += (s, e) => _output.Play();
        }

        public void SetTempo(int bpm)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(60000.0 / bpm);
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
    }
}