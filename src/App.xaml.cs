using System;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Model;
using Dock.Model.Controls;
using MQTTSniffer.Extensions;
using MQTTSniffer.ViewModels;
using MQTTSniffer.Views;
using ReactiveUI.Legacy;
using Splat;

namespace MQTTSniffer
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void RegisterServices()
        {
            base.RegisterServices();
            Bootstrapper.Register(Locator.CurrentMutable, Locator.Current); // note that I'm passing Splat service locators as parameters
        }
        public override void OnFrameworkInitializationCompleted()
        {
            var mainWindowViewModel = new MainWindowViewModel(Locator.Current.GetService<IExtensionImporter>());

            var factory = new MQTTSnifferFactory();
            IDock? layout = null;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                string path = Path.Combine(AppContext.BaseDirectory, "Layout.json");
                if (File.Exists(path))
                {
                    //layout = mainWindowViewModel.Serializer.Load<RootDock>(path);
                }

                var mainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };

                // TODO: Restore main window position, size and state.

                mainWindowViewModel.Factory = factory;
                mainWindowViewModel.Layout = layout ?? mainWindowViewModel.Factory?.CreateLayout();

                if (mainWindowViewModel.Layout != null)
                {
                    mainWindowViewModel.Factory?.InitLayout(mainWindowViewModel.Layout);
                }

                mainWindow.Closing += (sender, e) =>
                {
                    if (mainWindowViewModel.Layout is IDock dock)
                    {
                        dock.Close();
                    }
                    // TODO: Save main window position, size and state.
                };

                desktopLifetime.MainWindow = mainWindow;

                desktopLifetime.Exit += (sennder, e) =>
                {
                    if (mainWindowViewModel.Layout is IDock dock)
                    {
                        dock.Close();
                    }
                    //mainWindowViewModel.Serializer.Save(path, mainWindowViewModel.Layout);
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
            {
                var mainView = new MainView()
                {
                    DataContext = mainWindowViewModel
                };

                mainWindowViewModel.Factory = factory;
                mainWindowViewModel.Layout = layout ?? mainWindowViewModel.Factory?.CreateLayout();

                if (mainWindowViewModel.Layout != null)
                {
                    mainWindowViewModel.Factory?.InitLayout(mainWindowViewModel.Layout);
                }

                singleViewLifetime.MainView = mainView;
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
