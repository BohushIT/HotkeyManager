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

namespace HotkeyManager.Services
{
    public class WindowService : IWindowService
    {
        public async Task<Hotkey> ShowAddEditDialog(Hotkey hotkeyToEdit = null)
        {
            var window = new AddEditWindow();
            var viewModel = new AddEditViewModel(window, hotkeyToEdit); // Передаємо window і hotkeyToEdit
            window.DataContext = viewModel;

            Hotkey result = null;
            viewModel.HotkeySaved += (sender, hotkey) => result = hotkey;

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
