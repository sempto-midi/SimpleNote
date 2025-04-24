using SimpleNote.Plugins;
using System;
using System.Reflection;

namespace SimpleNote.Audio
{
    public static class NativePluginLoader
    {
        public static IPlugin Load(string path)
        {
            var assembly = Assembly.LoadFrom(path);
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type))
                {
                    return (IPlugin)Activator.CreateInstance(type);
                }
            }
            throw new InvalidOperationException("No IPlugin implementation found in assembly");
        }
    }
}