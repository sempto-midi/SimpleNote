using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SimpleNote
{
    /// <summary>
    /// Логика взаимодействия для Playlist.xaml
    /// </summary>
    public partial class Playlist : Page
    {
        private const int KeyHeight = 20; // Высота каждой клавиши
        private const int OctaveCount = 5; // Количество октав
        private const int KeysPerOctave = 12; // Количество клавиш в октаве
        private const int TotalKeys = OctaveCount * KeysPerOctave + 1; // Всего клавиш (65)
        private const int TotalTakts = 200; // Общее количество тактов

        private Line playheadMarker; // Маркер воспроизведения
        private double currentPosition = 0; // Текущая позиция маркера
        private DispatcherTimer playbackTimer; // Таймер для движения маркера
        private bool isPlaying = false; // Флаг воспроизведения
        private int tempo = 120; // Темп в BPM
        private double secondsPerBeat; // Секунд на удар
        private double pixelsPerSecond; // Пикселей в секунду
        public Playlist()
        {
            InitializeComponent();
            Loaded += Playlist_Loaded;
        }

        private void Playlist_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGridLines();
            DrawTaktNumbers();
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
                PlaylistCanvas.Children.Add(horizontalLine);
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
                PlaylistCanvas.Children.Add(verticalLine);
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

        private bool isResizing = false; // Флаг, указывающий, что пользователь изменяет размер прямоугольника
        private Rectangle currentRectangle = null; // Текущий редактируемый прямоугольник
        private int currentTaktIndex = 0; // Индекс начального такта для изменения ширины
        private List<Rectangle> createdRectangles = new List<Rectangle>(); // Список созданных прямоугольников
        private void PlaylistCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Определяем позицию клика
            var position = e.GetPosition(PlaylistCanvas);

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
                PlaylistCanvas.Children.Add(newRectangle);

                // Сохраняем созданный прямоугольник
                createdRectangles.Add(newRectangle);

                // Проверяем, находится ли курсор на правой границе прямоугольника для изменения ширины
                CheckResizeMode(newRectangle, position);
            }
        }

        private void PlaylistCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                // Определяем текущую позицию мыши
                var position = e.GetPosition(PlaylistCanvas);

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
                    if (IsOnRightEdge(rect, e.GetPosition(PlaylistCanvas)))
                    {
                        Mouse.OverrideCursor = Cursors.SizeWE; // Изменяем курсор на "изменение размера"
                        return;
                    }
                }

                // Возвращаем стандартный курсор
                Mouse.OverrideCursor = null;
            }
        }

        private void PlaylistCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
