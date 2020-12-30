using Avalonia.Input;

namespace MQTTSniffer.ViewModels
{
    public interface IDropTarget
    {
        void DragOver(object? sender, DragEventArgs e);
        void Drop(object? sender, DragEventArgs e);
    }
}
