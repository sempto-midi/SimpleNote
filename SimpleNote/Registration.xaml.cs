using SimpleNote.Data;
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
using System.Windows.Shapes;
using BCrypt.Net;
using SimpleNote.Models;

namespace SimpleNote
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            // Получаем значения из полей ввода
            string username = Username.Text.Trim();
            string email = Email.Text.Trim();
            string password = Password.Password;
            string confirmPassword = ConfirmPassword.Password;

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка подтверждения пароля
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new AppDbContext())
            {
                // Проверка существования пользователя с таким же именем или email
                if (context.Users.Any(u => u.Username == username || u.Email == email))
                {
                    MessageBox.Show("Username or email already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Хэшируем пароль
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                // Создаем нового пользователя
                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = hashedPassword,
                    Role = "User" // По умолчанию роль "User"
                    
                };

                // Добавляем пользователя в базу данных
                context.Users.Add(newUser);
                context.SaveChanges();

                // Открываем окно подтверждения email (или другое действие после успешной регистрации)
                ConfirmEmail confemWin = new ConfirmEmail(username, email);
                this.Close();
                confemWin.Show();
            }
        }

        private void HaveAnAcc_Click(object sender, RoutedEventArgs e)
        {
            MainWindow authWin = new MainWindow();
            this.Close();
            authWin.Show();
        }

        private void Username_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Username.Text == "Username")
            {
                Username.Text = string.Empty;
                Username.Foreground = Brushes.Black;
            }
        }

        private void Email_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Email.Text == "E-mail")
            {
                Email.Text = string.Empty;
                Email.Foreground = Brushes.Black;
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

        private void ConfirmPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ConfirmPassword.Password == "password")
            {
                ConfirmPassword.Password = string.Empty;
                ConfirmPassword.Foreground = Brushes.Black;
            }
        }

        private void SignUp_MouseDown(object sender, MouseButtonEventArgs e)
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
    }
}
