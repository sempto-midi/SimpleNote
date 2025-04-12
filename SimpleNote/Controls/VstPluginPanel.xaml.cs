using System.Windows;
using System.Windows.Controls;

namespace SimpleNote.Controls
{
    public partial class VstPluginPanel : UserControl
    {
        public event EventHandler RemoveRequested;

        public VstPluginPanel()
        {
            InitializeComponent();
        }

        public void SetPluginName(string name)
        {
            PluginNameText.Text = name;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            RemoveRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}