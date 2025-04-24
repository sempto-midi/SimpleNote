using SimpleNote.Plugins;
using System.Collections.ObjectModel;

namespace SimpleNote.Audio
{
    public class PluginManager
    {
        public ObservableCollection<IPlugin> Plugins { get; } = new ObservableCollection<IPlugin>();

        public void LoadVstPlugin(string pluginPath)
        {
            var plugin = new VstPluginHost(pluginPath)
            {
                ChannelId = Plugins.Count + 1
            };
            Plugins.Add(plugin);
        }

        public void UnloadPlugin(IPlugin plugin)
        {
            plugin.Dispose();
            Plugins.Remove(plugin);
        }
    }
}