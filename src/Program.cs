using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Logging;

namespace MQTTSniffer
{
    internal class Program
    {
        public static ILoggerFactory MyLoggerFactory;

        [STAThread]
        private static void Main(string[] args)
        {
            MyLoggerFactory = LoggerFactory.Create(options =>
            {
                options.AddConsole();
            });

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
