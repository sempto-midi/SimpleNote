using SimpleNote.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SimpleNote.Models;
using System.Net.Mail;
using System.Windows.Media.Animation;

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
            InitializeLoadingAnimation();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Анимация появления WELCOME блока
            var welcomeFadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1.8),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(welcomeFadeIn, WelcomeBlock);
            Storyboard.SetTargetProperty(welcomeFadeIn, new PropertyPath(OpacityProperty));

            var sb = new Storyboard();
            sb.Children.Add(welcomeFadeIn);

            sb.Completed += (s, args) =>
            {
                // 2. Анимация перемещения WELCOME блока
                var moveAnimation = new DoubleAnimation
                {
                    From = 400,
                    To = 40,
                    Duration = TimeSpan.FromSeconds(3),
                    EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
                };

                // 3. Анимация появления блока регистрации (начинается через 1 сек после начала движения)
                var registrationFadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    BeginTime = TimeSpan.FromSeconds(1.3), // Задержка перед стартом
                    Duration = TimeSpan.FromSeconds(1.5),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                // Запускаем обе анимации
                WelcomeTranslate.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
                RegistrationBlock.BeginAnimation(OpacityProperty, registrationFadeIn);
            };

            sb.Begin();
        }

        private void InitializeLoadingAnimation()
        {
            var rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever
            };
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Запускаем асинхронную операцию без блокировки UI
                await RegisterUserAsync();
            }
            catch (Exception ex)
            {
                var msg = new CustomMessageBox(this, ex.Message);
                msg.ShowDialog();
            }
        }

        private async Task RegisterUserAsync()
        {
            var username = Username.Text.Trim();
            var email = Email.Text.Trim();
            var password = Password.Password;
            var confirmPassword = ConfirmPassword.Password;

            // Проверка в UI-потоке перед началом асинхронных операций
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword)) throw new Exception("please fill in all fields");
            if (username.Length < 5) throw new Exception("username is too short (min 5)");
            if (!IsValidEmail(email)) throw new Exception("invalid email format");
            if (password.Length <= 5) throw new Exception("password is too short (min 6)");
            if (password != confirmPassword) throw new Exception("passwords do not match");
            // Основную логику выполняем в фоновом потоке
            await Task.Run(async () =>
            {
                using (var context = new AppDbContext())
                {
                    if (context.Users.Any(u => u.Username == username))
                        throw new Exception("username already exists");

                    if (context.Users.Any(u => u.Email == email))
                        throw new Exception("e-mail already registered");

                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                    var newUser = new User
                    {
                        Username = username,
                        Email = email,
                        PasswordHash = hashedPassword,
                        Role = "User"
                    };

                    context.Users.Add(newUser);
                    await context.SaveChangesAsync(); // Асинхронное сохранение

                    // Переход в UI-поток для показа нового окна
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var confirmWin = new ConfirmEmail(
                            newUser.Username,
                            newUser.Email,
                            newUser.UserId
                        );
                        this.Close();
                        confirmWin.Show();
                    });
                }
            });
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void HaveAnAcc_Click(object sender, RoutedEventArgs e)
        {
            MainWindow authWin = new MainWindow();
            this.Close();
            authWin.Show();
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
