using Avalonia;
using Avalonia.Controls;
using HotkeyManager.Commands;
using HotkeyManager.Models;
using HotkeyManager.Services.Interfaces;
using HotkeyManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HotkeyManager.Services
{
    public class SystemTrayService
    {
        private readonly ICommand _openCommand;
        private readonly ICommand _exitCommand;

        private TrayIcon _trayIcon;
        private readonly MainWindowViewModel _viewModel;
        private readonly IProcessService _processService;


        public SystemTrayService(ICommand openCommand, ICommand exitCommand, MainWindowViewModel viewModel, IProcessService processService)
        {
            _openCommand = openCommand;
            _exitCommand = exitCommand;
            _viewModel = viewModel;
            _processService = processService;
        }

        public void Initialize()
        {
            var subMenu = new NativeMenu();
            foreach (var name in _viewModel.Hotkeys)
            {
                var programPath = name.ProgramPath;
                subMenu.Add(new NativeMenuItem
                {
                    Header = name.ProgramPathString,
                    Command = new RelayCommand(() => 
                    {
                        try
                        {
                            if (_processService.IsProcessRunning(programPath))
                            {
                                _processService.TryActivateWindow(programPath);
                            }
                            else
                            {
                                _processService.StartProcess(programPath);

                            }
                        }
                        catch (Exception ex) { }
                    })

                });
                
            }

            _trayIcon = new TrayIcon
            {
                Icon = new WindowIcon("Assets/MainIcon.ico"),
                ToolTipText = "Hotkey Manager",
                Menu = new NativeMenu
                {
                new NativeMenuItem { Header = "Відкрити програму", Command = _openCommand },
                new NativeMenuItem { Header = "Закрити програму", Command = _exitCommand },
                new NativeMenuItem { Header = "ПоказатиЙ файли", Menu = subMenu }
          

            }
            };
            TrayIcon.SetIcons(Application.Current, new TrayIcons { _trayIcon });
        }

        public void Dispose()
        {
            _trayIcon?.Dispose();
        }
    }
}
