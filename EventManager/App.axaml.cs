using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EventManager.ViewModels;
using EventManager.Views;
using Microsoft.Extensions.Logging;

namespace EventManager
{
    public class App : Application
    {
        private readonly ILoggerFactory _loggerFactory;

        public App()
        {

        }
        public App(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(_loggerFactory.CreateLogger("MainWindow")),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
