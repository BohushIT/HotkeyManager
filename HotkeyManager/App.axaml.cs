using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HotkeyManager.Repositories;
using HotkeyManager.Services;
using HotkeyManager.ViewModels;

namespace HotkeyManager
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow();
                var repository = new JsonHotkeyRepository();
                var windowService = new WindowService();
                var processService = new ProcessService();
                mainWindow.DataContext = new MainWindowViewModel(repository, windowService, processService);
                desktop.MainWindow = mainWindow;
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}