using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors; // Если используешь NuGet пакет Behaviors

namespace SimpleNote
{
    public class PasswordBoxPlaceholderBehavior : Behavior<PasswordBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PasswordChanged += OnPasswordChanged;
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.LostFocus += OnLostFocus;
            UpdatePlaceholderVisibility();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PasswordChanged -= OnPasswordChanged;
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderVisibility();
        }

        private void UpdatePlaceholderVisibility()
        {
            if (AssociatedObject.Template.FindName("placeholderText", AssociatedObject) is TextBlock placeholder)
            {
                placeholder.Visibility =
                    string.IsNullOrEmpty(AssociatedObject.Password) && !AssociatedObject.IsKeyboardFocused
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }
}