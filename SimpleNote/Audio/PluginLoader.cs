using SimpleNote.Plugins;
using System;
using System.IO;
using System.Reflection;

namespace SimpleNote.Audio
{
    public static class PluginLoader
    {
        public static IPlugin LoadPlugin(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Plugin file not found", path);

            var assembly = Assembly.LoadFrom(path);
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type))
                {
                    return (IPlugin)Activator.CreateInstance(type);
                }
            }
            throw new InvalidOperationException($"No valid IPlugin implementation found in {path}");
        }
    }
}