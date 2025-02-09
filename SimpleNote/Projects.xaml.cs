using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleNote
{
    /// <summary>
    /// Логика взаимодействия для Projects.xaml
    /// </summary>
    public class FileInfo
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public DateTime LastWriteTime { get; set; }

    }
    public partial class Projects : Window
    {
        private string _username;
        public Projects(string username)
        {
            InitializeComponent();
            LoadFiles(@"C:\radio");
            _username = username;
            UpdateWelcomeMessage();
        }

        // Метод для обновления текста приветствия
        private void UpdateWelcomeMessage()
        {
            // Находим TextBlock с именем "StartText" и обновляем его текст
            var startText = FindName("StartText") as TextBlock;
            if (startText != null)
            {
                startText.Text = $"LET'S GO, {_username}!";
            }
        }

        private void LoadFiles(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath)
                    .Select(file => new FileInfo
                    {
                        Path = file,
                        Name = Path.GetFileName(file),
                        LastWriteTime = File.GetLastWriteTime(file)
                    })
                    .OrderByDescending(file => file.LastWriteTime)
                    .ToList();

                FilesList.Children.Clear();

                var todayFiles = files.Where(file => file.LastWriteTime.Date == DateTime.Today).ToList();
                var recentlyFiles = files.Where(file => file.LastWriteTime.Date >= DateTime.Today.AddDays(-2) && file.LastWriteTime.Date < DateTime.Today).ToList();
                var earlierFiles = files.Where(file => file.LastWriteTime.Date < DateTime.Today.AddDays(-2)).ToList();

                AddFileGroup("Today", todayFiles);
                AddFileGroup("Recently", recentlyFiles);
                AddFileGroup("Earlier", earlierFiles);
            }
            else
            {
                MessageBox.Show($"Folder {folderPath} does not exist.");
            }
        }

        private void AddFileGroup(string header, List<FileInfo> files)
        {
            if (files.Any())
            {
                // Добавляем заголовок
                var headerTextBlock = new TextBlock
                {
                    Text = header,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 20,
                    Margin = new Thickness(0, 10, 0, 5)
                };
                FilesList.Children.Add(headerTextBlock);

                // Добавляем файлы
                foreach (var file in files)
                {
                    var fileButton = new Button
                    {
                        Content = file.Name, // Название файла
                        Tag = file.Path, // Передаем путь к файлу в Tag
                        Style = (Style)FindResource("FileButtonStyle")
                    };

                    fileButton.Click += FileButton_Click;
                    FilesList.Children.Add(fileButton);
                }
            }
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string filePath)
            {
                // Проверяем, существует ли файл
                if (File.Exists(filePath))
                {
                    // Открываем окно ... и передаем путь к выбранному файлу
                    
                }
                else
                {
                    MessageBox.Show($"Файл не найден: {filePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Projects1_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            Workspace workspace = new Workspace();
            this.Close();
            workspace.Show();
        }
    }
}
