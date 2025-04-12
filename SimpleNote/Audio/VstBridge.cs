using System;
using System.Runtime.InteropServices;

namespace SimpleNote.Audio
{
    public unsafe class VstBridge : IDisposable
    {
        private IntPtr _pluginHandle;
        private delegate* unmanaged[Stdcall]<IntPtr, uint, int, int, int, int, int> _dispatcher;

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllPath);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        public void LoadPlugin(string pluginPath)
        {
            _pluginHandle = LoadLibrary(pluginPath);
            if (_pluginHandle == IntPtr.Zero)
                throw new Exception($"Failed to load VST plugin: {pluginPath}");

            IntPtr mainPtr = GetProcAddress(_pluginHandle, "VSTPluginMain");
            if (mainPtr == IntPtr.Zero)
                throw new Exception("Not a valid VST plugin");

            _dispatcher = (delegate* unmanaged[Stdcall]<IntPtr, uint, int, int, int, int, int>)mainPtr;
        }

        public void ProcessAudio(float[] buffer, int sampleCount)
        {
            fixed (float* bufferPtr = buffer)
            {
                _dispatcher(_pluginHandle, 0xDEADBEEF, 0, sampleCount, (int)bufferPtr, 0);
            }
        }

        public void Dispose()
        {
            if (_pluginHandle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_pluginHandle);
                _pluginHandle = IntPtr.Zero;
            }
        }
    }
}