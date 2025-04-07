using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HotkeyManager.Models;
using HotkeyManager.Services;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using HotkeyManager.Commands;
using HotkeyManager.Repositories.Interfaces;
using HotkeyManager.Services.Interfaces;

//dotnet publish -c Release -r linux-x64 --self-contained
//dotnet publish HotkeyManager.csproj -c Release -r linux-x64 --self-contained -o Release


namespace HotkeyManager.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IProcessService _processService;
        private string _status = "Очікую натискання клавіш...";
        private readonly IHotkeyRepository _repository;
        private ObservableCollection<Hotkey> _hotkeys;
        private ModifierMask _activeModifiers = ModifierMask.None; 
        private Hotkey _selectedHotkey;
        private readonly IWindowService _windowService;
        private bool _disposed = false;
        public MainWindowViewModel( IHotkeyRepository repository, IWindowService windowService, IProcessService processService)
        {
           
            _repository = repository;
            _windowService = windowService;
            _processService = processService;
            _hotkeys = new ObservableCollection<Hotkey>();

            GlobalHookService.Instance.KeyPressed += OnKeyPressed;
            GlobalHookService.Instance.KeyReleased += OnKeyReleased;

            _ = InitializeAsync();

            AddHotkeyCommand = new RelayCommand(async () => await AddHotkeyAsync());
            RemoveHotkeyCommand = new RelayCommand(async () => await RemoveHotkeyAsync());
            EditHotkeyCommand = new RelayCommand(async () => await EditHotkeyAsync());
   
        }

        public MainWindowViewModel()
        {
        }
        private async Task InitializeAsync()
        {
            try
            {
                var loadedHotkeys = await _repository.LoadAsync();
                foreach (var hotkey in loadedHotkeys) 
                {
                    _hotkeys.Add(hotkey);
                }
                Status = $"Програма запущена, завантажено {Hotkeys.Count} комбінацій";
            }
            catch (Exception ex)
            {
                Status = $"Помилка: {ex.Message}";
            }
        }
        public Hotkey SelectedHotkey
        {
            get => _selectedHotkey;
            set { _selectedHotkey = value; OnPropertyChanged(); }
        }
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Hotkey> Hotkeys
        {
            get => _hotkeys;
            set
            {
                _hotkeys = value;
                OnPropertyChanged(); 
            }
        }
        public ICommand AddHotkeyCommand { get; }
        public ICommand RemoveHotkeyCommand { get; }
        public ICommand EditHotkeyCommand { get; }

        
        private async Task AddHotkeyAsync()
        {
            
            Hotkey result = await _windowService.ShowAddEditDialog();

            if (result != null)
            {
                await _repository.AddHotkeyAsync(result);
                _hotkeys = await _repository.LoadAsync();
                OnPropertyChanged(nameof(Hotkeys));
                Status = "Додано нову комбінацію";
            }
            else
            {
                Status = "Додавання скасовано";
            }
        }

        private async Task RemoveHotkeyAsync()
        {


            if (SelectedHotkey == null) 
            {
                Status = "Виберіть комбінацію для Видалення";
                return;
            }
        
            var hotkeyToRemove = SelectedHotkey;

            bool confirmed = await _windowService.ShowConfirmationDialog(
                            "Підтвердження",
                            "Ви впевнені, що хочете видалити цю комбінацію?");

            if (confirmed == true) 
            {
                await _repository.RemoveHotkeyAsync(hotkeyToRemove);
                _hotkeys = await _repository.LoadAsync();
                OnPropertyChanged(nameof(Hotkeys));
                Status = "Комбінацію видалено";
            }
            else 
            {
                Status = "Видалення скасоване";
            }
        }

        private async Task EditHotkeyAsync()
        {
            if (SelectedHotkey == null) 
            {
                Status = "Виберіть комбінацію для редагування";
                return;
            }

            Hotkey result = await _windowService.ShowAddEditDialog(SelectedHotkey); 

            if (result != null)
            {
                await _repository.EditHotKeyAsync(SelectedHotkey, result);
                _hotkeys = await _repository.LoadAsync();
                OnPropertyChanged(nameof(Hotkeys));
                Status = "Оновлено комбінацію";
            }
            else
            {
                Status = "Редагування скасовано";
            }
        }

        private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            
            if (IsModifierKey(e.Data.KeyCode))
            {
                _activeModifiers |= GetModifierFromKey(e.Data.KeyCode); 
                return;
            }

            
            if (_activeModifiers != ModifierMask.None) 
            {
                var pressedKey = e.Data.KeyCode;
                var matchingHotkey = _hotkeys.FirstOrDefault(h =>
                    h.Modifier == _activeModifiers && h.Key == pressedKey);

                if (matchingHotkey != null)
                {
                    var programPath = matchingHotkey.ProgramPath;
                    if (_processService.IsProcessRunning(programPath) && !matchingHotkey.RunMultipleInstances)
                    {
                        Status = $"{Path.GetFileName(programPath)} вже запущено";
                        return;
                    }

                    Status = $"Виконується: {matchingHotkey.Modifier} + {matchingHotkey.Key} -> {matchingHotkey.ProgramPath}";
                    _processService.StartProcess(programPath);
                }
                else
                {
                    Status = $"Натиснуто: {pressedKey} (Модифікатор: {_activeModifiers}) - комбінація не знайдена";
                }
            }
        }
        
        private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            
            if (IsModifierKey(e.Data.KeyCode))
            {
                _activeModifiers &= ~GetModifierFromKey(e.Data.KeyCode); 
            }
        }

        private bool IsModifierKey(KeyCode keyCode)
        {
            return keyCode == KeyCode.VcLeftControl ||
                   keyCode == KeyCode.VcRightControl ||
                   keyCode == KeyCode.VcLeftAlt ||
                   keyCode == KeyCode.VcRightAlt ||
                   keyCode == KeyCode.VcLeftShift ||
                   keyCode == KeyCode.VcRightShift ||
                   keyCode == KeyCode.VcLeftMeta ||
                   keyCode == KeyCode.VcEscape ||
                   keyCode == KeyCode.VcRightMeta ||
                   keyCode == KeyCode.VcTab;
        }

        private ModifierMask GetModifierFromKey(KeyCode keyCode)
        {
            return keyCode switch
            {
                KeyCode.VcLeftControl => ModifierMask.LeftCtrl,
                KeyCode.VcRightControl => ModifierMask.RightCtrl,
                KeyCode.VcLeftAlt => ModifierMask.LeftAlt,
                KeyCode.VcRightAlt => ModifierMask.RightAlt,
                KeyCode.VcLeftShift => ModifierMask.LeftShift,
                KeyCode.VcRightShift => ModifierMask.RightShift,
                KeyCode.VcLeftMeta => ModifierMask.LeftMeta,
                KeyCode.VcRightMeta => ModifierMask.RightMeta,
                _ => ModifierMask.None
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    File.AppendAllText("hotkeymanager_log.txt", " Початок очистки\n");
                    if (GlobalHookService.Instance != null)
                    {
                        GlobalHookService.Instance.KeyPressed -= OnKeyPressed;
                        GlobalHookService.Instance.KeyReleased -= OnKeyReleased;
                        GlobalHookService.Instance.Dispose();
                    }
                    File.AppendAllText("hotkeymanager_log.txt", " Кінець очистки\n");
                }
                catch (Exception ex)
                {
                    File.AppendAllText("hotkeymanager_log.txt", $" Помилка в Dispose: {ex.Message}\n");
                }
                _disposed = true;
            }
        }
    }
}
