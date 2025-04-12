using NAudio.CoreAudioApi;
using NAudio.Midi;
using NAudio.Mixer;
using NAudio.Vst;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

public partial class Workspace : Window
{
    private List<VstPluginContext> _plugins = new List<VstPluginContext>();
    private WaveOutEvent _outputDevice;
    private MixingSampleProvider _mixer;
    private Metronome _metronome;
    private Dictionary<int, AudioChannel> _audioChannels = new Dictionary<int, AudioChannel>();

    public Workspace()
    {
        InitializeComponent();
        InitializeAudioEngine();
        InitializeMetronome();
        Loaded += Workspace_Loaded;
    }

    private void Workspace_Loaded(object sender, RoutedEventArgs e)
    {
        CreateDefaultChannels(8); // Создаем 8 каналов по умолчанию
    }

    private void InitializeAudioEngine()
    {
        _outputDevice = new WaveOutEvent();
        _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2))
        {
            ReadFully = true
        };
        _outputDevice.Init(_mixer);
        _outputDevice.Play();
    }

    private void InitializeMetronome()
    {
        _metronome = new Metronome(120); // Стандартный BPM
        _mixer.AddMixerInput(_metronome.GetAudioProvider());
    }

    private void CreateDefaultChannels(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddChannel($"Channel {i + 1}");
        }
    }

    private void AddChannel(string name)
    {
        int channelId = _audioChannels.Count + 1;

        // Добавляем в Channel Rack
        var channelControl = new ChannelControl(channelId, this);
        ChannelRackPanel.Children.Add(channelControl);

        // Добавляем в Mixer
        var mixerControl = new MixerControl(channelId);
        MixerPanel.Children.Add(mixerControl);

        // Создаем аудиоканал
        var audioChannel = new AudioChannel();
        _audioChannels.Add(channelId, audioChannel);
        _mixer.AddMixerInput(audioChannel.GetAudioProvider());
    }

    // Метод для изменения BPM
    public void SetBpm(int bpm)
    {
        _metronome.SetTempo(bpm);
        Tempo.Text = bpm.ToString();
    }

    // Метод для добавления VST плагина
    public void AddPluginToChannel(int channelId, string pluginPath)
    {
        if (_audioChannels.TryGetValue(channelId, out var channel))
        {
            try
            {
                var plugin = VstPluginContext.Create(pluginPath);
                _plugins.Add(plugin);
                channel.AddPlugin(plugin);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки плагина: {ex.Message}");
            }
        }
    }

    // Метод для изменения громкости канала
    public void SetChannelVolume(int channelId, float volume)
    {
        if (_audioChannels.TryGetValue(channelId, out var channel))
        {
            channel.SetVolume(volume);
        }
    }
}