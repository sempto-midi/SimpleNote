using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SimpleNote.Audio
{
    public class AudioEngine : IDisposable
    {
        private readonly WaveOut _waveOut;
        private readonly MixingSampleProvider _mixer;
        private readonly List<VstBridge> _plugins = new List<VstBridge>();
        private BufferedWaveProvider _bufferedWaveProvider;

        public AudioEngine()
        {
            _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
            {
                ReadFully = true
            };
            _waveOut = new WaveOut();
            _waveOut.Init(_mixer);
            _waveOut.Play();
        }

        public void AddPlugin(string pluginPath)
        {
            var plugin = new VstBridge();
            plugin.LoadPlugin(pluginPath);
            _plugins.Add(plugin);
        }

        public void ProcessAudio(float[] buffer)
        {
            foreach (var plugin in _plugins)
            {
                plugin.ProcessAudio(buffer, buffer.Length / 2);
            }

            // Create a new SampleProvider for the processed buffer and add it to the mixer
            var sampleProvider = new BufferedSampleProvider(_mixer.WaveFormat);
            sampleProvider.AddSamples(buffer, 0, buffer.Length);
            _mixer.AddMixerInput(sampleProvider);
        }

        public void Dispose()
        {
            _waveOut.Dispose();
            foreach (var plugin in _plugins)
            {
                plugin.Dispose();
            }
        }
    }

    // Helper class to convert buffers to ISampleProvider
    public class BufferedSampleProvider : ISampleProvider
    {
        private readonly WaveFormat _waveFormat;
        private readonly Queue<float> _sampleQueue = new Queue<float>();

        public BufferedSampleProvider(WaveFormat waveFormat)
        {
            _waveFormat = waveFormat;
        }

        public WaveFormat WaveFormat => _waveFormat;

        public void AddSamples(float[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                _sampleQueue.Enqueue(buffer[offset + i]);
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = 0;
            while (samplesRead < count && _sampleQueue.Count > 0)
            {
                buffer[offset + samplesRead++] = _sampleQueue.Dequeue();
            }
            return samplesRead;
        }
    }
}