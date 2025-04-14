using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HotkeyManager.Repositories;
using HotkeyManager.Repositories.Interfaces;
using HotkeyManager.Services;
using HotkeyManager.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows.Input;
using HotkeyManager.Commands;
using HotkeyManager.Services.Interfaces;
using HotkeyManager.Platform.Interfaces;
using System.Runtime.InteropServices;
using HotkeyManager.Platform.Windows;
using HotkeyManager.Platform.Linux;

namespace HotkeyManager
{
    public partial class App : Application
    {
        private MainWindow _mainWindow;
        private IServiceProvider _serviceProvider;


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

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _serviceProvider = ConfigureServices();
                _mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                desktop.MainWindow = _mainWindow;

                desktop.ShutdownRequested += (sender, e) =>
                {
                    if (_mainWindow.DataContext is MainWindowViewModel viewModel)
                    {
                        viewModel.Dispose();
                        File.AppendAllText("hotkeymanager_log.txt", " Очищення при завершенні програми ShutdownRequested \n");
                    }
                    _serviceProvider.GetRequiredService<SystemTrayService>().Dispose();
                };

                var viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                var trayService = _serviceProvider.GetRequiredService<SystemTrayService>();
                await viewModel.InitializeAsync(); // Чекаємо завершення
                trayService.Initialize();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddScoped<IHotkeyRepository, JsonHotkeyRepository>();
            services.AddScoped<IWindowService, WindowService>(); 
            services.AddScoped<IProcessService, ProcessService>(); 
            services.AddScoped<MainWindowViewModel>();
            services.AddTransient<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainWindowViewModel>()
            });
            services.AddTransient<AddEditViewModel>();
            services.AddSingleton<IProcessActivator>(sp =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? new WindowsProcessActivator()
                    : new LinuxProcessActivator());
            services.AddSingleton<SystemTrayService>(sp => new SystemTrayService(
                OpenCommand, 
                ExitCommand,
                sp.GetRequiredService<MainWindowViewModel>(),
               sp.GetRequiredService<IProcessService>()));

            return services.BuildServiceProvider();
        }

        private void OpenApplication()
        {
            if (_mainWindow == null) return;

            if (_mainWindow.WindowState == WindowState.Minimized)
            {
                _mainWindow.WindowState = WindowState.Normal;
            }

            if (_mainWindow.IsVisible)
            {
                _mainWindow.Activate();
            }
            else
            {
                _mainWindow.Show();
            }

            _mainWindow.BringIntoView();
        }

        private void ExitApplication()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
                if (_mainWindow.DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.Dispose();
                    File.AppendAllText("hotkeymanager_log.txt", " Очищення при завершенні програми ShutdownRequested \n");
                }
            }
        }
    }
}