using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NAudio.Midi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SimpleNote
{
    public partial class PianoRoll : Page
    {
        private const int KeyHeight = 20; // Высота каждой клавиши
        private const int OctaveCount = 5; // Количество октав
        private const int KeysPerOctave = 12; // Количество клавиш в октаве
        private const int TotalKeys = OctaveCount * KeysPerOctave + 1; // Всего клавиш (65)
        private const int TotalTakts = 200; // Общее количество тактов

        private List<string> noteNames = new List<string> { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        private Dictionary<int, Button> keyButtons = new Dictionary<int, Button>(); // Словарь для хранения клавиш
        private MidiIn midiIn; // MIDI-устройство
        private WaveOutEvent waveOut; // Для воспроизведения звука
        private SignalGenerator signalGenerator; // Генератор звука

        public PianoRoll()
        {
            InitializeComponent();
            Loaded += PianoRollPage_Loaded; // Подписываемся на событие загрузки страницы
        }

        private void PianoRollPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializePianoKeys();
            DrawGridLines();
            DrawTaktNumbers(); // Добавляем номера тактов
            InitializeAudio(); // Инициализация аудио
            InitializeMidi(); // Инициализация MIDI
        }

        private void InitializePianoKeys()
        {
            for (int i = TotalKeys - 1; i >= 0; i--)
            {
                int octave = i / KeysPerOctave;
                int noteIndex = i % KeysPerOctave;
                string noteName = noteNames[noteIndex] + (octave + 3); // Начинаем с C3

                // Создание кнопки для клавиши
                Button keyButton = new Button
                {
                    Height = KeyHeight,
                    Content = noteName,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Gray,
                    Background = noteName.Contains("#") ? Brushes.Black : Brushes.White,
                    Foreground = noteName.Contains("#") ? Brushes.White : Brushes.Black,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                // Обработка нажатия на клавишу
                int midiNote = 60 + i; // MIDI-нота (C3 = 60)
                keyButton.Click += (sender, e) => PlayNote(midiNote);

                // Добавление кнопки в панель
                PianoKeysPanel.Children.Add(keyButton);
                keyButtons[midiNote] = keyButton; // Сохраняем кнопку в словаре
            }
        }

        private void InitializeAudio()
        {
            waveOut = new WaveOutEvent();
            signalGenerator = new SignalGenerator()
            {
                Type = SignalGeneratorType.Pink, // Тип звука (синусоида)
                Gain = 0.2, // Громкость
                Frequency = 440 // Частота (A4)
            };
            waveOut.Init(signalGenerator);
        }

        private void InitializeMidi()
        {
            if (MidiIn.NumberOfDevices > 0)
            {
                midiIn = new MidiIn(0); // Используем первое MIDI-устройство
                midiIn.MessageReceived += MidiIn_MessageReceived; // Обработка MIDI-сообщений
                midiIn.Start();
            }
            else
            {
                MessageBox.Show("MIDI-устройство не найдено.");
            }
        }

        private void MidiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            if (e.MidiEvent.CommandCode == MidiCommandCode.NoteOn)
            {
                var noteEvent = (NoteEvent)e.MidiEvent;
                int midiNote = noteEvent.NoteNumber;

                // Воспроизведение ноты
                Dispatcher.Invoke(() => PlayNote(midiNote));

                // Подсветка клавиши
                Dispatcher.Invoke(() => HighlightKey(midiNote, true));
            }
            else if (e.MidiEvent.CommandCode == MidiCommandCode.NoteOff)
            {
                var noteEvent = (NoteEvent)e.MidiEvent;
                int midiNote = noteEvent.NoteNumber;

                // Снятие подсветки клавиши
                Dispatcher.Invoke(() => HighlightKey(midiNote, false));
            }
        }

        private void PlayNote(int midiNote)
        {
            // Установка частоты для генератора звука
            signalGenerator.Frequency = MidiNoteToFrequency(midiNote);

            // Воспроизведение звука
            if (waveOut.PlaybackState != PlaybackState.Playing)
            {
                waveOut.Play();
            }

            // Подсветка клавиши
            HighlightKey(midiNote, true);

            // Снятие подсветки через 500 мс
            Task.Delay(500).ContinueWith(_ => Dispatcher.Invoke(() => HighlightKey(midiNote, false)));
        }

        private void HighlightKey(int midiNote, bool highlight)
        {
            if (keyButtons.ContainsKey(midiNote))
            {
                var keyButton = keyButtons[midiNote];
                keyButton.Background = highlight ? Brushes.Yellow : (keyButton.Content.ToString().Contains("#") ? Brushes.Black : Brushes.White);
            }
        }

        private double MidiNoteToFrequency(int midiNote)
        {
            // Формула для преобразования MIDI-ноты в частоту
            return 440.0 * Math.Pow(2, (midiNote - 69) / 12.0);
        }

        private void DrawGridLines()
        {
            // Разлиновка по клавишам
            for (int i = 0; i < TotalKeys; i++)
            {
                Line horizontalLine = new Line
                {
                    X1 = 0,
                    X2 = TotalTakts * 100,
                    Y1 = i * KeyHeight,
                    Y2 = i * KeyHeight,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5
                };
                PianoRollCanvas.Children.Add(horizontalLine);
            }

            // Разлиновка по тактам (примерно каждые 4 клавиши)
            for (int i = 0; i < TotalTakts * 100; i += 100)
            {
                Line verticalLine = new Line
                {
                    X1 = i,
                    X2 = i,
                    Y1 = 0,
                    Y2 = TotalKeys * KeyHeight,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5
                };
                PianoRollCanvas.Children.Add(verticalLine);
            }
        }

        private void DrawTaktNumbers()
        {
            for (int i = 1; i <= TotalTakts; i++)
            {
                TextBlock taktNumber = new TextBlock
                {
                    Text = i.ToString(),
                    Width = 100, // Ширина одного такта
                    TextAlignment = TextAlignment.Center,
                    Foreground = (i % 4 == 0) ? Brushes.Orange : Brushes.LightGray, // Оранжевый для каждого 4-го такта
                    FontSize = (i % 4 == 0) ? 14 : 12 // Увеличенный шрифт для каждого 4-го такта
                };

                TaktNumbersPanel.Children.Add(taktNumber);
            }
        }

        private void LeftScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                MainScrollViewer.ScrollToVerticalOffset(LeftScrollViewer.VerticalOffset);
            }
        }

        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                LeftScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset);
            }
        }

        private void TaktNumbersPanel_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Отменяем стандартное поведение прокрутки
            e.Handled = true;

            // Определяем направление прокрутки
            double delta = e.Delta > 0 ? -30 : 30; // Изменение на 30 пикселей

            // Меняем горизонтальное смещение MainScrollViewer
            MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset - delta);
        }
    }
}