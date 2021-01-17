using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MQTTSniffer.ViewModels.Tools;
using ReactiveUI;

namespace MQTTSniffer.Views.Tools
{
    public class FindView : UserControl, IViewFor<FindViewModel>
    {
        public FindView()
        {
            this.InitializeComponent();
        }

        public FindViewModel ViewModel { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        object IViewFor.ViewModel { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
