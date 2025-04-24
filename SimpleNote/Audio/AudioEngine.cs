using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SimpleNote.Plugins;

namespace SimpleNote.Audio
{
    public class AudioEngine : IDisposable
    {
        private readonly WaveOutEvent _waveOut = new WaveOutEvent();
        private readonly MixingSampleProvider _finalMixer;
        private readonly Dictionary<int, MixingSampleProvider> _channelMixers = new Dictionary<int, MixingSampleProvider>();
        private readonly PluginManager _pluginManager;

        public AudioEngine(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
            _finalMixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
            {
                ReadFully = true
            };

            _waveOut.Init(_finalMixer);
            _waveOut.Play();

            _pluginManager.Plugins.CollectionChanged += (s, e) => RebuildAudioGraph();
        }

        public void PlayNote(int midiNote, int channel)
        {
            if (!_channelMixers.TryGetValue(channel, out var channelMixer))
            {
                channelMixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
                _channelMixers[channel] = channelMixer;
                RebuildAudioGraph();
            }

            var plugin = _pluginManager.Plugins.FirstOrDefault(p => p.ChannelId == channel);
            if (plugin == null) return;

            var generator = new SignalGenerator
            {
                Type = SignalGeneratorType.Sin,
                Frequency = MidiNoteToFrequency(midiNote),
                Gain = plugin.Volume
            };

            var processed = plugin.ProcessAudio(generator);
            channelMixer.AddMixerInput(processed);
        }

        private void RebuildAudioGraph()
        {
            _finalMixer.RemoveAllMixerInputs();

            foreach (var channel in _channelMixers)
            {
                _finalMixer.AddMixerInput(channel.Value);
            }
        }

        private double MidiNoteToFrequency(int midiNote)
        {
            return 440.0 * Math.Pow(2, (midiNote - 69) / 12.0);
        }

        public void Dispose()
        {
            _waveOut?.Dispose();
        }
    }
}