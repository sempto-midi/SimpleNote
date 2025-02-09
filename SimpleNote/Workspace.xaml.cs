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
            Tempo.Text = _currentValue.ToString();
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

        private void PianoRoll_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("PianoRoll.xaml", UriKind.Relative));
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Uri("Reload.xaml", UriKind.Relative));

        }
        //private void OpenPlaylist_Click(object sender, RoutedEventArgs e)
        //{
        //    MainFrame.Navigate(new Uri("Playlist.xaml", UriKind.Relative));
        //}
    }
}
