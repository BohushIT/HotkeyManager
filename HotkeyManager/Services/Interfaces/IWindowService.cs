using Avalonia.Controls;
using HotkeyManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotkeyManager.Services.Interfaces
{
    public interface IWindowService
    {

        Task<Hotkey> ShowAddEditDialog(Hotkey hotkeyToEdit = null);
        Task<bool> ShowConfirmationDialog(string title, string message);
    }
}
