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

            this.DataContextChanged += BrokerView_DataContextChanged;
        }

        private void BrokerView_DataContextChanged(object? sender, System.EventArgs e)
        {
            var combobox = this.FindControl<ComboBox>("BrokerSelection");
            combobox.SelectionChanged += (DataContext as MainWindowViewModel).BrokerSelectionChanged;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
