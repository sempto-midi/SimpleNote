using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
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

namespace SimpleNote
{
    /// <summary>
    /// Логика взаимодействия для ConfirmEmail.xaml
    /// </summary>
    public partial class ConfirmEmail : Window
    {
        private string _username;
        private string _email;
        private string _confirmationCode;

        public ConfirmEmail(string username, string email)
        {
            InitializeComponent();
            FirstSymb.Focus();
            _username = username;
            _email = email;

            GenerateAndSendConfirmationCode();
        }

        private string GenerateConfirmationCode()
        {
            Random random = new Random();
            return random.Next(10000, 99999).ToString(); // Генерирует число от 10000 до 99999
        }

        // Отправка письма с кодом подтверждения
        private void GenerateAndSendConfirmationCode()
        {
            try
            {
                // Генерация кода
                _confirmationCode = GenerateConfirmationCode();

                // Настройка SMTP-клиента
                using (SmtpClient smtpClient = new SmtpClient("smtp.mail.ru", 587))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new NetworkCredential("s1mplenote@mail.ru", "1KRapgDfzreMdhkgQbgk");

                    // Создание письма
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress("s1mplenote@mail.ru"),
                        Subject = "Your Confirmation Code",
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(_email);

                    // Чтение HTML-шаблона
                    string htmlTemplate = @"
                        <!DOCTYPE html>
                        <html lang=""en"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Confirmation Code</title>
                            <style>
                                body {
                                    background-color: #393939;
                                    color: white;
                                    margin: 0;
                                    padding: 0;
                                    display: flex;
                                    justify-content: center;
                                    align-items: center;
                                    height: 100vh;
                                }
                                .container {
                                    text-align: center;
                                    max-width: 600px;
                                    padding: 20px;
                                    background-color: #252525;
                                    border-radius: 10px;
                                }
                                .logo {
                                    max-width: 100px;
                                    margin-bottom: 20px;
                                }
                                .code {
                                    font-size: 30px;
                                    border: 2px solid white;
                                    border-radius: 5px;
                                    padding: 10px;
                                    display: inline-block;
                                    margin-top: 20px;
                                }
                            </style>
                        </head>
                        <body>
                            <div class=""container"">
                                <img src=""cid:logo"" alt=""Logo"" class=""logo"">
                                <p>Hello, {username}!</p>
                                <p>Your confirmation code is:</p>
                                <div class=""code"">{confirmationCode}</div>
                            </div>
                        </body>
                        </html>";

                    // Замена переменных в шаблоне
                    string body = htmlTemplate.Replace("{username}", _username).Replace("{confirmationCode}", _confirmationCode);

                    // Добавление логотипа как встроенного ресурса
                    LinkedResource logoResource = new LinkedResource("C:\\Users\\Denzel\\source\\repos\\SimpleNote\\SimpleNote\\pics\\logo.png");
                    logoResource.ContentId = "logo";
                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                    htmlView.LinkedResources.Add(logoResource);
                    mailMessage.AlternateViews.Add(htmlView);

                    // Отправка письма
                    smtpClient.Send(mailMessage);
                }

                MessageBox.Show($"A confirmation code has been sent to {_email}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send confirmation code. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Проверка введенного кода
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            // Собираем введенный код из текстовых полей
            string enteredCode = $"{FirstSymb.Text}{SecondSymb.Text}{ThirdSymb.Text}{FourthSymb.Text}{FifthSymb.Text}";

            if (string.IsNullOrEmpty(enteredCode) || enteredCode.Length != 5)
            {
                MessageBox.Show("Please enter a valid 5-digit code.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Сравниваем введенный код с сохраненным
            if (enteredCode == _confirmationCode)
            {
                MessageBox.Show("Code confirmed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Переход к следующему окну
                Projects projWin = new Projects(_username);
                this.Close();
                projWin.Show();
            }
            else
            {
                MessageBox.Show("Incorrect confirmation code. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FirstSymb_TextChanged(object sender, TextChangedEventArgs e)
        {
            SecondSymb.Focus();
        }

        private void SecondSymb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ThirdSymb.Focus();
        }

        private void ThirdSymb_TextChanged(object sender, TextChangedEventArgs e)
        {
            FourthSymb.Focus();
        }

        private void FourthSymb_TextChanged(object sender, TextChangedEventArgs e)
        {
            FifthSymb.Focus();
        }

        private void FirstSymb_GotFocus(object sender, RoutedEventArgs e)
        {
            if(FirstSymb.Text !=  null) FirstSymb.SelectAll();
        }

        private void SecondSymb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SecondSymb.Text != null) SecondSymb.SelectAll();
        }

        private void ThirdSymb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ThirdSymb.Text != null) ThirdSymb.SelectAll();
        }

        private void FourthSymb_GotFocus(object sender, RoutedEventArgs e)
        {
            if(FourthSymb.Text != null) FourthSymb.SelectAll();
        }

        private void FifthSymb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FifthSymb.Text != null) FifthSymb.SelectAll();
        }

        private void ConfirmEmail1_MouseDown(object sender, MouseButtonEventArgs e)
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
