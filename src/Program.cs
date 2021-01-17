using System;
using Avalonia;
using Avalonia.ReactiveUI;

namespace MQTTSniffer
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
                .LogToTrace()
                .UseReactiveUI();
    }
}
