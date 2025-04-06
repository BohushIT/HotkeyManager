using Avalonia.Media.TextFormatting.Unicode;
using HotkeyManager.Models;
using HotkeyManager.Repositories.Interfaces;
using HotkeyManager.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HotkeyManager.Repositories
{
    public class JsonHotkeyRepository : IHotkeyRepository
    {
        private readonly string _filePath;

        public JsonHotkeyRepository(IConfigurationService configurationService)
        {
            _filePath = configurationService.GetJsonFilePath();
        }

        public async Task SaveAsync(ObservableCollection<Hotkey> hotkeys)
        {
            string json = JsonSerializer.Serialize(hotkeys, new JsonSerializerOptions { WriteIndented= true});
           await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<ObservableCollection<Hotkey>> LoadAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new ObservableCollection<Hotkey>();
            }

            string json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<ObservableCollection<Hotkey>>(json) ?? new ObservableCollection<Hotkey>();
        }

        public async Task AddHotkeyAsync(Hotkey hotkey)
        {
            var hotkeys = await LoadAsync();
            int newId =  hotkeys.Any() ? hotkeys.Max(h => h.Id) + 1 : 1;
            hotkey.Id = newId;
            var existingHotkey = hotkeys.FirstOrDefault(
                h =>
                h.Modifier ==hotkey.Modifier &&
                h.Key == hotkey.Key);
            if (existingHotkey == null) 
            {
                hotkeys.Add(hotkey);
               await SaveAsync(hotkeys);
            }


        }
        public async Task RemoveHotkeyAsync(Hotkey hotkey) 
        {
            var hotkeys = await LoadAsync();
            var itemToRemove = hotkeys.FirstOrDefault(
                h =>h.Id == hotkey.Id);
            if (itemToRemove != null) 
            {
                hotkeys.Remove(itemToRemove);
                await SaveAsync(hotkeys);
            }

        }

        public async Task EditHotKeyAsync(Hotkey oldHotkey, Hotkey newHotkey)
        {
            var hotkeys = await LoadAsync();
            int indexToEdit = hotkeys.ToList().FindIndex(h => h.Id == oldHotkey.Id); // Перетворюємо в List і шукаємо індекс
            if (indexToEdit != -1)
            {
                newHotkey.Id = oldHotkey.Id;
                hotkeys[indexToEdit] = newHotkey;
                await SaveAsync(hotkeys);
            }
        }
    }
}
