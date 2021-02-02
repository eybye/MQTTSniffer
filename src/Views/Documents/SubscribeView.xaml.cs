using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MQTTSniffer.ViewModels.Documents;

namespace MQTTSniffer.Views.Documents
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
