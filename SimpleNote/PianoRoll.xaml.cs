using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private Dictionary<int, SignalGenerator> activeNotes = new Dictionary<int, SignalGenerator>(); // Словарь для отслеживания активных нот

        private MidiIn midiIn; // MIDI-устройство
        private WaveOutEvent waveOut; // Для воспроизведения звука

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
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Tag = 60 + i // Присваиваем тег с MIDI-нотой
                };

                // Обработка нажатия мыши
                keyButton.PreviewMouseDown += Button_MouseDown;
                keyButton.PreviewMouseUp += Button_MouseUp;

                // Добавление кнопки в панель
                PianoKeysPanel.Children.Add(keyButton);
                keyButtons[60 + i] = keyButton; // Сохраняем кнопку в словаре
            }
        }

        private void InitializeAudio()
        {
            waveOut = new WaveOutEvent();
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
            }
            else if (e.MidiEvent.CommandCode == MidiCommandCode.NoteOff)
            {
                var noteEvent = (NoteEvent)e.MidiEvent;
                int midiNote = noteEvent.NoteNumber;

                // Остановка ноты
                Dispatcher.Invoke(() => StopNote(midiNote));
            }
        }

        private void PlayNote(int midiNote)
        {
            if (!activeNotes.ContainsKey(midiNote))
            {
                // Создаем новый генератор звука для этой ноты
                SignalGenerator signalGenerator = new SignalGenerator
                {
                    Type = SignalGeneratorType.Sin,
                    Gain = 0.2,
                    Frequency = MidiNoteToFrequency(midiNote)
                };

                // Добавляем генератор в словарь активных нот
                activeNotes[midiNote] = signalGenerator;

                // Если это первая нота или есть изменения в сигналах, обновляем вывод
                UpdateAudioOutput();

                // Подсветка клавиши
                HighlightKey(midiNote, true);
            }
        }

        private void StopNote(int midiNote)
        {
            if (activeNotes.ContainsKey(midiNote))
            {
                // Удаляем генератор звука из словаря активных нот
                activeNotes.Remove(midiNote);

                // Если больше нет активных нот, останавливаем воспроизведение
                if (activeNotes.Count == 0)
                {
                    waveOut.Stop();
                }
                else
                {
                    // Обновляем вывод при наличии других активных нот
                    UpdateAudioOutput();
                }

                // Снятие подсветки клавиши
                HighlightKey(midiNote, false);
            }
        }

        private void UpdateAudioOutput()
        {
            if (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Stop(); // Останавливаем текущее воспроизведение
            }

            var mixedSignal = MixMultipleSignals();

            if (mixedSignal != null)
            {
                waveOut.Init(mixedSignal); // Инициализируем новый смешанный сигнал
                waveOut.Play(); // Начинаем воспроизведение
            }
        }

        private ISampleProvider MixMultipleSignals()
        {
            if (activeNotes.Count == 0) return null;

            // Создаем список всех активных генераторов звука
            var generators = activeNotes.Values.ToList();

            // Используем MixingSampleProvider для смешивания всех сигналов
            var mixingProvider = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
            mixingProvider.ReadFully = true;

            foreach (var generator in generators)
            {
                mixingProvider.AddMixerInput(generator);
            }

            return mixingProvider;
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

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int midiNote))
            {
                PlayNote(midiNote);
            }
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int midiNote))
            {
                StopNote(midiNote);
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

        private void TaktNumbersPanel_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Отменяем стандартное поведение прокрутки
            e.Handled = true;
            // Определяем направление прокрутки
            double delta = e.Delta > 0 ? -30 : 30; // Изменение на 30 пикселей
            // Меняем горизонтальное смещение MainScrollViewer
            MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset - delta);
        }

        private bool isDrawing = false; // Флаг, указывающий, что пользователь рисует прямоугольник
        private Rectangle currentRectangle = null; // Текущий рисуемый прямоугольник
        private int currentRectStartX = 0; // Начальная X-координата прямоугольника
        private int currentRectStartY = 0; // Начальная Y-координата прямоугольника
        private int currentKeyIndex = 0; // Индекс начальной клавиши
        private int currentTaktIndex = 0; // Индекс начального такта
        private List<Rectangle> createdRectangles = new List<Rectangle>(); // Список созданных прямоугольников

        private void PianoRollCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Определяем начальную точку прямоугольника
            var position = e.GetPosition(PianoRollCanvas);

            int startKeyIndex = (int)(position.Y / KeyHeight); // Индекс клавиши
            int startTaktIndex = (int)(position.X / 100); // Индекс такта

            if (startKeyIndex >= 0 && startKeyIndex < TotalKeys && startTaktIndex >= 0 && startTaktIndex < TotalTakts)
            {
                // Сохраняем начальные координаты
                currentRectStartX = startTaktIndex * 100;
                currentRectStartY = startKeyIndex * KeyHeight;

                // Создаем новый прямоугольник
                currentRectangle = new Rectangle
                {
                    Stroke = Brushes.Green,
                    Fill = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0)), // Полупрозрачный зеленый цвет
                    StrokeThickness = 1
                };

                // Добавляем его на Canvas
                PianoRollCanvas.Children.Add(currentRectangle);

                // Устанавливаем начальные координаты
                Canvas.SetLeft(currentRectangle, currentRectStartX);
                Canvas.SetTop(currentRectangle, currentRectStartY);

                // Отслеживаем движение мыши
                isDrawing = true;
            }
        }

        private void PianoRollCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                // Определяем текущую позицию мыши
                var position = e.GetPosition(PianoRollCanvas);

                int endKeyIndex = (int)(position.Y / KeyHeight); // Индекс клавиши
                int endTaktIndex = (int)(position.X / 100); // Индекс такта

                if (endKeyIndex >= 0 && endKeyIndex < TotalKeys && endTaktIndex >= 0 && endTaktIndex < TotalTakts)
                {
                    // Вычисляем размер прямоугольника
                    int width = Math.Max(100, (endTaktIndex - currentTaktIndex + 1) * 100);
                    int height = Math.Max(KeyHeight, Math.Abs((endKeyIndex - currentKeyIndex + 1) * KeyHeight));

                    // Обновляем размеры прямоугольника
                    currentRectangle.Width = width;
                    currentRectangle.Height = height;

                    // Если конечная клавиша выше начальной, меняем положение
                    if (endKeyIndex < currentKeyIndex)
                    {
                        Canvas.SetTop(currentRectangle, endKeyIndex * KeyHeight);
                    }

                    // Если конечный такт слева от начального, меняем положение
                    if (endTaktIndex < currentTaktIndex)
                    {
                        Canvas.SetLeft(currentRectangle, endTaktIndex * 100);
                    }
                }
            }
        }

        private void PianoRollCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing)
            {
                // Завершаем рисование
                isDrawing = false;

                // Сохраняем созданный прямоугольник
                createdRectangles.Add(currentRectangle);

                // Сбрасываем текущий прямоугольник
                currentRectangle = null;
            }
        }

    }
}