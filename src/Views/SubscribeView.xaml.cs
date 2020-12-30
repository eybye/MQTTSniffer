using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MQTTSniffer.Views
{
    public class SubscribeView : UserControl
    {
        public SubscribeView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
