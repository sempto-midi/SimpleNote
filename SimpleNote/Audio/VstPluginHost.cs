using ManagedBass;
using ManagedBass.Vst;
using SimpleNote.Plugins;
using System;
using System.Runtime;
using System.Windows;
using System.Windows.Controls;

namespace SimpleNote.Audio
{
    public class VstPluginHost : IPlugin
    {
        private int _vstHandle;
        private int _streamHandle;
        private readonly VstInfo _info;

        public string Name { get; }
        public int ChannelId { get; set; }
        public float Volume { get; set; } = 0.7f;

        public VstPluginHost(string pluginPath)
        {
            // Инициализация Bass
            if (!Bass.Init())
                throw new Exception($"Bass init failed: {Bass.LastError}");

            // Создаем поток для обработки аудио
            _streamHandle = Bass.CreateStream(44100, 2, BassFlags.Default, StreamProcedureType.Push);
            if (_streamHandle == 0)
                throw new Exception($"Stream create failed: {Bass.LastError}");

            // Загружаем VST плагин
            _vstHandle = BassVst.Load(_streamHandle, pluginPath, VstFlags.None);
            if (_vstHandle == 0)
                throw new Exception($"VST load failed: {Bass.LastError}");

            // Получаем информацию о плагине
            _info = BassVst.GetInfo(_vstHandle);
            Name = _info.EffectName;

            // Запускаем обработку аудио
            Bass.ChannelPlay(_streamHandle);
        }

        public void ProcessAudio(float[] samples)
        {
            // Отправляем MIDI-событие (пример)
            BassVst.ProcessEvent(_vstHandle, new VstEvent
            {
                EventType = VstEventType.Midi,
                MidiNote = 60, // Средняя C
                Channel = ChannelId
            });

            // Обрабатываем аудио
            Bass.StreamPutData(_streamHandle, samples, samples.Length * sizeof(float));
        }

        public UIElement GetEditorView()
        {
            var panel = new StackPanel();

            // Кнопка для открытия редактора VST
            var btn = new Button
            {
                Content = "Open Editor",
                Margin = new Thickness(5)
            };
            btn.Click += (s, e) => BassVst.OpenEditor(_vstHandle);

            // Слайдер громкости
            var slider = new Slider
            {
                Minimum = 0,
                Maximum = 1,
                Value = Volume,
                Margin = new Thickness(5),
                Width = 200
            };
            slider.ValueChanged += (s, e) => {
                Volume = (float)e.NewValue;
                BassVst.SetParam(_vstHandle, 0, Volume); // Параметр 0 - обычно громкость
            };

            panel.Children.Add(new TextBlock
            {
                Text = Name,
                Margin = new Thickness(5),
                FontWeight = FontWeights.Bold
            });
            panel.Children.Add(btn);
            panel.Children.Add(slider);

            return panel;
        }

        public void Dispose()
        {
            if (_vstHandle != 0)
            {
                BassVst.Free(_vstHandle);
                _vstHandle = 0;
            }

            if (_streamHandle != 0)
            {
                Bass.StreamFree(_streamHandle);
                _streamHandle = 0;
            }

            Bass.Free();
        }
    }
}