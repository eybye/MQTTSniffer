using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace MQTTSniffer.Dialogs
{
    public class EditBrokerDialog : Window
    {
        public EditBrokerDialog()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnSaveCommand(object sender, RoutedEventArgs e)
        {
            Close(Constants.SAVE);
        }
        private void OnCancelCommand(object sender, RoutedEventArgs e)
        {
            Close(Constants.CANCEL);
        }
    }
}
