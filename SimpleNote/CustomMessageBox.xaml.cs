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
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SimpleNote
{
    /// <summary>
    /// Логика взаимодействия для CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(Window owner, string message)
        {
            InitializeComponent();
            this.Owner = owner;
            MessageText.Text = message;
            Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Анимация появления
            var opacityAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut }
            };

            var marginAnim = new ThicknessAnimation
            {
                From = new Thickness(400, 130, 400, 130),
                To = new Thickness(0),
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            this.BeginAnimation(OpacityProperty, opacityAnim);
            MainBorder.BeginAnimation(MarginProperty, marginAnim);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OkButton.IsEnabled = false;

            // Анимация закрытия (зеркальная открытию)
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseIn }
            };

            var marginOutAnim = new ThicknessAnimation
            {
                From = new Thickness(0),
                To = new Thickness(400, 130, 400, 130),
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            // Создаем Storyboard для синхронизации анимаций
            var closingStoryboard = new Storyboard();
            closingStoryboard.Children.Add(fadeOut);
            closingStoryboard.Children.Add(marginOutAnim);

            Storyboard.SetTarget(fadeOut, this);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));

            Storyboard.SetTarget(marginOutAnim, MainBorder);
            Storyboard.SetTargetProperty(marginOutAnim, new PropertyPath(MarginProperty));

            closingStoryboard.Completed += (s, args) =>
            {
                this.DialogResult = true;
                this.Close();
            };

            closingStoryboard.Begin();
        }
    }
}
