using SimpleNote.Plugins;
using System.Windows;

namespace SimpleNote.Views
{
    public partial class PluginSelectionDialog : Window
    {
        public IPlugin SelectedPlugin { get; private set; }

        public PluginSelectionDialog()
        {
            InitializeComponent();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPlugin = PluginsList.SelectedItem as IPlugin;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}