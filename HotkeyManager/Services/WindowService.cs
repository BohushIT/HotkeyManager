using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HotkeyManager.Models;
using HotkeyManager.ViewModels;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MsBox.Avalonia.Dto;
using HotkeyManager.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HotkeyManager.Services
{
    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<Hotkey> ShowAddEditDialog(Hotkey hotkeyToEdit = null)
        {
            var viewModel = _serviceProvider.GetRequiredService<AddEditViewModel>();
            viewModel.Initialize(hotkeyToEdit);

            var window = new AddEditWindow();
          
            window.DataContext = viewModel;

            Hotkey result = null;
            bool shouldClose = false;
            viewModel.HotkeySaved += (sender, hotkey) =>
            {
                result = hotkey;

                if (viewModel.ShouldClose) 
                {
                    window.Close();
                }
            };

            var parent = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (parent == null) return null;
            await window.ShowDialog(parent);
            return result;
        }

        public async Task<bool> ShowConfirmationDialog(string title, string message)
        {
            var parent = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (parent == null) return false;

            var messageBox = MessageBoxManager
                .GetMessageBoxStandard(new MessageBoxStandardParams
                {
                    ContentTitle = title,
                    ContentMessage = message,
                    ButtonDefinitions = ButtonEnum.YesNo, 
                    Icon = Icon.Question, 
                    WindowStartupLocation = WindowStartupLocation.CenterOwner, 
                    CanResize = false, 
                    SystemDecorations = SystemDecorations.Full, 
                  
                    FontFamily = "Segoe UI", 
                    
                });

            var result = await messageBox.ShowWindowDialogAsync(parent);
            return result == ButtonResult.Yes;
        }
    }
}
