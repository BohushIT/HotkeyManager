using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HotkeyManager.Repositories;
using HotkeyManager.Services;
using HotkeyManager.ViewModels;
using System.Windows.Input;
using System;
using HotkeyManager.Commands;

namespace HotkeyManager
{
    public partial class App : Application
    {
        private MainWindow _mainWindow;

        // Команди для трею
        public ICommand OpenCommand { get; }
        public ICommand ExitCommand { get; }

        public App()
        {
            OpenCommand = new RelayCommand(OpenApplication);
            ExitCommand = new RelayCommand(ExitApplication);
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _mainWindow = new MainWindow();
                var repository = new JsonHotkeyRepository();
                var windowService = new WindowService();
                var processService = new ProcessService();
                var viewModel = new MainWindowViewModel( repository, windowService, processService);
                _mainWindow.DataContext = viewModel;
                desktop.MainWindow = _mainWindow;

                // Створюємо TrayIcon
                var trayIcon = new TrayIcon
                {
                    Icon = new WindowIcon("D:\\TestLinux\\HotkeyManager\\HotkeyManager\\Assets\\MainIcon.ico"),
                    ToolTipText = "Hotkey Manager",
                    Menu = new NativeMenu
                    {
                        new NativeMenuItem { Header = "Відкрити програму", Command = OpenCommand },
                        new NativeMenuItem { Header = "Закрити програму", Command = ExitCommand }
                    }
                };

                // Встановлюємо TrayIcon
                TrayIcon.SetIcons(this, new TrayIcons { trayIcon });
            }
            base.OnFrameworkInitializationCompleted();
        }

        private void OpenApplication()
        {
            if (_mainWindow == null) return; // Захист від null

            if (_mainWindow.WindowState == WindowState.Minimized)
            {
                _mainWindow.WindowState = WindowState.Normal; // Розгортаємо, якщо згорнуте
            }

            if (_mainWindow.IsVisible)
            {
                _mainWindow.Activate(); // Активуємо, якщо вже видно
            }
            else
            {
                _mainWindow.Show(); // Показуємо, якщо приховано
            }

            // Додатково: Явно фокусуємо вікно
            _mainWindow.BringIntoView();
        }

        private void ExitApplication()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        
    }
}