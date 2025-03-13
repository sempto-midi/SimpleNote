using System;
using System.Collections.Generic;
using System.IO;
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
using Path = System.IO.Path;

namespace SimpleNote
{
    /// <summary>
    /// Логика взаимодействия для Workspace.xaml
    /// </summary>
    public partial class Workspace : Window
    {
        private bool _isFullscreen = false;

        public Workspace()
        {
            InitializeComponent();
            Tempo.Text = "120";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinMax_Click(object sender, RoutedEventArgs e)
        {
            if (_isFullscreen)
            {
                // Возвращаем окно в нормальное состояние
                this.WindowState = WindowState.Normal;
            }
            else
            {
                // Переключаем окно в полноэкранный режим
                this.WindowState = WindowState.Maximized;
            }

            // Инвертируем флаг
            _isFullscreen = !_isFullscreen;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(!(WindowState.Equals(WindowState.Maximized)))
            {
                Rectangle rectangle = new Rectangle
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.White,
                    StrokeThickness = 1
                };
                MinMax.Content = rectangle;
            }
            else
            {
                MinMax.Content = new Image
                {
                    Width = 10,
                    Height = 10,
                    Source = new BitmapImage(new Uri("pics/MinWin.png", UriKind.Relative))
                };

            }
        }

        private bool _isDragging = false; // Флаг для отслеживания зажатия ЛКМ
        private Point _startPoint;       // Начальная позиция курсора
        private int _currentValue = 120;   // Текущее числовое значение

        //Обработка нажатия ЛКМ
        private void Tempo_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _startPoint = e.GetPosition(this); // Сохраняем начальную позицию курсора
                Tempo.CaptureMouse();     // Захватываем мышь для отслеживания движения
            }
        }

        private void Tempo_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this); // Текущая позиция курсора
                double deltaY = _startPoint.Y - currentPoint.Y; // Разница по оси Y

                // Изменяем значение с шагом в 1
                int deltaValue = (int)Math.Round(deltaY);
                _currentValue += deltaValue;

                _currentValue = Math.Max(30, Math.Min(300, _currentValue)); // Ограничение от 0 до 100

                // Обновляем текстовое поле
                Tempo.Text = _currentValue.ToString();

                // Обновляем начальную точку для следующего шага
                _startPoint = currentPoint;
            }
        }

        private void Tempo_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Tempo.ReleaseMouseCapture(); // Освобождаем захват мыши
            }
        }

        private void Tempo_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Увеличиваем или уменьшаем значение в зависимости от направления прокрутки
            _currentValue += Math.Sign(e.Delta);

            _currentValue = Math.Max(30, Math.Min(300, _currentValue)); // Ограничение от 0 до 100

            // Обновляем текстовое поле
            Tempo.Text = _currentValue.ToString();
        }

        private void Tempo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(Tempo.Text, out int newTempo))
            {
                // Обновляем BPM в PianoRoll, если он существует
                _pianoRollPage?.SetTempo(newTempo);
            }
        }

        public void UpdateTime(string timeString)
        {
            if (TimerTextBox != null)
            {
                TimerTextBox.Text = timeString;
            }
        }

        private PianoRoll _pianoRollPage; // Ссылка на страницу PianoRoll

        private void PianoRoll_Click(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр PianoRoll и сохраняем ссылку
            _pianoRollPage = new PianoRoll();
            MainFrame.Navigate(_pianoRollPage);

            // Передаем текущий темп на страницу PianoRoll
            if (_pianoRollPage != null && int.TryParse(Tempo.Text, out int currentTempo))
            {
                _pianoRollPage.SetTempo(currentTempo);
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем метод StartPlayback() на странице PianoRoll
            _pianoRollPage?.StartPlayback();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            _pianoRollPage?.PausePlayback();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _pianoRollPage?.StopPlayback();
        }

        private Playlist _playlistPage;
        private void Playlist_Click(object sender, RoutedEventArgs e)
        {
            if(_pianoRollPage.isPlaying == true)
            {
                _pianoRollPage.StopPlayback();
                _playlistPage = new Playlist();
                MainFrame.Navigate(_playlistPage);
            }
            else
            {
                _playlistPage = new Playlist();
                MainFrame.Navigate(_playlistPage);
            }
        }
    }
}
