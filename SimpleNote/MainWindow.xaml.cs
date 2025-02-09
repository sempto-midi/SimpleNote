using SimpleNote.Data;
using System.Text;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // Получаем значения из полей ввода
            string usernameOrEmail = Username.Text.Trim();
            string password = Password.Password;

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new AppDbContext())
            {
                // Поиск пользователя по имени пользователя или email
                var user = context.Users.FirstOrDefault(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);

                if (user == null)
                {
                    MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка пароля
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    MessageBox.Show("Incorrect password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Открываем окно проектов
                Projects projWin = new Projects(user.Username);
                this.Close();
                projWin.Show();
            }
        }

        private void DontHaveAcc_Click(object sender, RoutedEventArgs e)
        {
            Registration regWin = new Registration();
            this.Close();
            regWin.Show();
        }

        private void Username_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Username.Text == "username")
            {
                Username.Text = string.Empty;
                Username.Foreground = Brushes.Black;
            }
        }

        private void Password_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Password.Password == "password")
            {
                Password.Password = string.Empty;
                Password.Foreground = Brushes.Black;
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Authorization_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}