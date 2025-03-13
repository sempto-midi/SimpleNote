using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
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

        private Line playheadMarker; // Маркер воспроизведения
        private double currentPosition = 0; // Текущая позиция маркера
        private DispatcherTimer playbackTimer; // Таймер для движения маркера
        public bool isPlaying = false; // Флаг воспроизведения
        private int tempo = 120; // Темп в BPM
        private double secondsPerBeat; // Секунд на удар
        private double pixelsPerSecond; // Пикселей в секунду

        private List<string> noteNames = new List<string> { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        private Dictionary<int, Button> keyButtons = new Dictionary<int, Button>(); // Словарь для хранения клавиш
        private Dictionary<int, SignalGenerator> activeNotes = new Dictionary<int, SignalGenerator>(); // Словарь для отслеживания активных нот

        private MidiIn midiIn; // MIDI-устройство
        private WaveOutEvent waveOut; // Для воспроизведения звука

        private Workspace _workspace; // Ссылка на родительское окно

        public PianoRoll()
        {
            InitializeComponent();
            Loaded += PianoRollPage_Loaded; // Подписываемся на событие загрузки страницы
            playbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // ~60 FPS
            playbackTimer.Tick += PlaybackTimer_Tick;
        }

        private void PianoRollPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializePianoKeys();
            DrawGridLines();
            DrawTaktNumbers();
            InitializeAudio();
            InitializeMidi();

            // Сохраняем ссылку на родительское окно
            var workspace = Window.GetWindow(this) as Workspace;
            if (workspace != null)
            {
                _workspace = workspace;
            }

            // Инициализируем таймер и зависимые значения
            secondsPerBeat = 60.0 / tempo;
            pixelsPerSecond = 100 / (secondsPerBeat * 4);

            // Создаем маркер воспроизведения
            playheadMarker = new Line
            {
                X1 = 0,
                X2 = 0,
                Y1 = 0,
                Y2 = TotalKeys * KeyHeight,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };
            PianoRollCanvas.Children.Add(playheadMarker);

            // Инициализируем таймер
            playbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // ~60 FPS
            playbackTimer.Tick += PlaybackTimer_Tick;
        }

        public void SetTempo(int newTempo)
        {
            tempo = newTempo;
            secondsPerBeat = 60.0 / tempo;
            pixelsPerSecond = 100 / (secondsPerBeat * 4);

            // Если воспроизведение идет, обновляем интервал таймера
            if (isPlaying)
            {
                playbackTimer.Interval = TimeSpan.FromSeconds(secondsPerBeat / 4);
            }
        }

        public void StartPlayback()
        {
            if (!isPlaying)
            {
                isPlaying = true;
                playbackTimer.Start();
            }
        }

        public void PausePlayback()
        {
            if (isPlaying)
            {
                isPlaying = false;
                playbackTimer.Stop();

                // Останавливаем все активные ноты
                foreach (var note in activeNotes.Keys.ToList())
                {
                    StopNote(note);
                }
            }
        }

        public void StopPlayback()
        {
            isPlaying = false;
            playbackTimer.Stop();
            currentPosition = 0;
            UpdatePlayheadPosition();

            // Очищаем таймер в Workspace
            _workspace?.UpdateTime("00:00:00");

            // Останавливаем все активные ноты
            foreach (var note in activeNotes.Keys.ToList())
            {
                StopNote(note);
            }
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (isPlaying)
            {
                currentPosition += pixelsPerSecond * 0.016; // Перемещаем маркер на основе темпа
                UpdatePlayheadPosition();

                if (currentPosition >= PianoRollCanvas.Width)
                {
                    isPlaying = false;
                    playbackTimer.Stop();
                    MessageBox.Show("Воспроизведение завершено!");
                }

                // Обновляем время в Workspace
                UpdateTimeInWorkspace();

                // Проигрываем или останавливаем ноты в зависимости от положения маркера
                UpdateActiveNotes();
            }
        }

        private void UpdateActiveNotes()
        {
            List<int> notesToStart = new List<int>();
            List<int> notesToStop = new List<int>();

            foreach (var rectangle in createdRectangles)
            {
                double rectX = Canvas.GetLeft(rectangle);
                double rectWidth = rectangle.Width;

                int keyIndex = (int)(Canvas.GetTop(rectangle) / KeyHeight);
                int midiNote = 60 + TotalKeys - 1 - keyIndex;

                if (currentPosition >= rectX && currentPosition < rectX + rectWidth)
                {
                    notesToStart.Add(midiNote);
                }
                else
                {
                    notesToStop.Add(midiNote);
                }
            }

            foreach (var note in notesToStart.Distinct())
            {
                PlayNote(note);
            }

            foreach (var note in notesToStop.Distinct())
            {
                StopNote(note);
            }

            var notesToRemove = activeNotes.Keys.ToList().Where(note => !notesToStart.Contains(note));
            foreach (var note in notesToRemove)
            {
                StopNote(note);
            }
        }

        private void UpdatePlayheadPosition()
        {
            Canvas.SetLeft(playheadMarker, currentPosition);
        }

        private void UpdateTimeInWorkspace()
        {
            double seconds = currentPosition / 100; // Конвертируем пиксели в секунды
            int minutes = (int)(seconds / 60);
            int remainingSeconds = (int)(seconds % 60);
            int milliseconds = (int)((seconds - Math.Floor(seconds)) * 1000);

            string timeString = $"{minutes:D2}:{remainingSeconds:D2}:{milliseconds:D3}";

            // Вызываем метод обновления времени в Workspace
            _workspace?.UpdateTime(timeString);
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

                // Обновляем аудио вывод
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

                // Обновляем аудио вывод
                UpdateAudioOutput();

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
            for (int i = 0; i < TotalTakts * 400; i += 100)
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
            if (e.HorizontalChange != 0)
            {
                // Синхронизируем горизонтальное смещение TaktNumbersScrollViewer с MainScrollViewer
                TaktNumbersScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset);
            }

            if (e.VerticalChange != 0)
            {
                // Синхронизируем вертикальное смещение LeftScrollViewer с MainScrollViewer
                LeftScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset);
            }
        }

        private void TaktNumbersScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.HorizontalChange != 0)
            {
                // Синхронизируем горизонтальное смещение MainScrollViewer с TaktNumbersScrollViewer
                MainScrollViewer.ScrollToHorizontalOffset(TaktNumbersScrollViewer.HorizontalOffset);
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

        private bool isResizing = false; // Флаг, указывающий, что пользователь изменяет размер прямоугольника
        private Rectangle currentRectangle = null; // Текущий редактируемый прямоугольник
        private int currentTaktIndex = 0; // Индекс начального такта для изменения ширины
        private List<Rectangle> createdRectangles = new List<Rectangle>(); // Список созданных прямоугольников

        private void PianoRollCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Определяем позицию клика
            var position = e.GetPosition(PianoRollCanvas);

            int keyIndex = (int)(position.Y / KeyHeight); // Индекс клавиши
            int taktIndex = (int)(position.X / 100); // Индекс такта

            if (keyIndex >= 0 && keyIndex < TotalKeys && taktIndex >= 0 && taktIndex < TotalTakts)
            {
                // Создаем новый прямоугольник
                Rectangle newRectangle = new Rectangle
                {
                    Stroke = Brushes.Green,
                    Fill = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0)), // Полупрозрачный зеленый цвет
                    StrokeThickness = 1,
                    Width = 100, // Ширина одного такта
                    Height = KeyHeight // Высота одной клавиши
                };

                // Устанавливаем положение прямоугольника
                Canvas.SetLeft(newRectangle, taktIndex * 100);
                Canvas.SetTop(newRectangle, keyIndex * KeyHeight);

                // Добавляем прямоугольник на Canvas
                PianoRollCanvas.Children.Add(newRectangle);

                // Сохраняем созданный прямоугольник
                createdRectangles.Add(newRectangle);

                // Проверяем, находится ли курсор на правой границе прямоугольника для изменения ширины
                CheckResizeMode(newRectangle, position);
            }
        }

        private void PianoRollCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                // Определяем текущую позицию мыши
                var position = e.GetPosition(PianoRollCanvas);

                int endTaktIndex = (int)(position.X / 100); // Индекс конечного такта

                if (endTaktIndex >= currentTaktIndex && endTaktIndex < TotalTakts)
                {
                    // Обновляем ширину прямоугольника
                    currentRectangle.Width = (endTaktIndex - currentTaktIndex + 1) * 100;
                }
            }
            else
            {
                // Проверяем, находится ли курсор на правой границе какого-либо прямоугольника
                foreach (var rect in createdRectangles)
                {
                    if (IsOnRightEdge(rect, e.GetPosition(PianoRollCanvas)))
                    {
                        Mouse.OverrideCursor = Cursors.SizeWE; // Изменяем курсор на "изменение размера"
                        return;
                    }
                }

                // Возвращаем стандартный курсор
                Mouse.OverrideCursor = null;
            }
        }

        private void PianoRollCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isResizing)
            {
                // Завершаем изменение размера
                isResizing = false;
                currentRectangle = null;
            }
        }

        private bool IsOnRightEdge(Rectangle rectangle, Point position)
        {
            double rectX = Canvas.GetLeft(rectangle);
            double rectY = Canvas.GetTop(rectangle);
            double rectWidth = rectangle.Width;

            // Проверяем, находится ли курсор на правой границе прямоугольника
            return position.X >= rectX + rectWidth - 5 && position.X <= rectX + rectWidth + 5 &&
                   position.Y >= rectY && position.Y <= rectY + rectangle.Height;
        }

        private void CheckResizeMode(Rectangle rectangle, Point position)
        {
            if (IsOnRightEdge(rectangle, position))
            {
                // Начинаем режим изменения размера
                isResizing = true;
                currentRectangle = rectangle;
                currentTaktIndex = (int)(Canvas.GetLeft(rectangle) / 100);
            }
        }

    }
}