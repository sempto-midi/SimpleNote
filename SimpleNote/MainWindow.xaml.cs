using SimpleNote.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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

            var nullColor = new SolidColorBrush(Color.FromArgb(187, 187, 187, 100));
            // Проверка на пустые поля
            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                var msg = new CustomMessageBox(this, "please fill in all fields");
                msg.ShowDialog();
                return;
            }

            using (var context = new AppDbContext())
            {
                // Поиск пользователя по имени пользователя или email
                var user = context.Users.FirstOrDefault(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);

                if (user == null)
                {
                    var msg = new CustomMessageBox(this, "user not found");
                    msg.ShowDialog();
                    return;
                }

                // Проверка пароля
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    var msg = new CustomMessageBox(this, "incorrect password");
                    msg.ShowDialog();
                    return;
                }

                // Открываем окно проектов с передачей username и userId
                Projects projWin = new Projects(user.Username, user.UserId);
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