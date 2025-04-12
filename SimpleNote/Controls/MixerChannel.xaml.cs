using System.Windows.Controls;

namespace SimpleNote
{
    public partial class MixerChannel : UserControl
    {
        public int ChannelNumber { get; set; }

        public MixerChannel(int channelNumber)
        {
            InitializeComponent();
            ChannelNumber = channelNumber;
            DataContext = this; // Для привязки ChannelNumber в XAML
        }
    }
}