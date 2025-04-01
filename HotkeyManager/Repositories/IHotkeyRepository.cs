using HotkeyManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotkeyManager.Repositories
{
    public interface IHotkeyRepository
    {
        Task SaveAsync(ObservableCollection<Hotkey> hotkeys);
        Task AddHotkeyAsync(Hotkey hotkey);
        Task RemoveHotkeyAsync(Hotkey hotkey);
        Task EditHotKeyAsync(Hotkey oldHotkey, Hotkey newHotkey);
        Task<ObservableCollection<Hotkey>> LoadAsync();
    }
}
