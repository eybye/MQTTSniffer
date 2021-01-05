using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using MQTTSniffer.ViewModels;

namespace MQTTSniffer.Views
{
    public class BrokerView : UserControl
    {
        public BrokerView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
