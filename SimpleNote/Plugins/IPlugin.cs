using NAudio.Wave;
using System.Windows;

namespace SimpleNote.Plugins
{
    public interface IPlugin : IDisposable
    {
        string Name { get; }
        string Version { get; }
        int ChannelId { get; set; }
        float Volume { get; set; }

        ISampleProvider ProcessAudio(ISampleProvider input);
        UIElement GetEditorView();
    }
}