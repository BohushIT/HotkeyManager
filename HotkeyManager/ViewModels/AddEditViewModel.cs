using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HotkeyManager.Models;
using SharpHook.Native;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using HotkeyManager.Commands;


namespace HotkeyManager.ViewModels
{
    public class AddEditViewModel : INotifyPropertyChanged
    {
      
        public ObservableCollection<ModifierMask> Modifiers { get; } = new ObservableCollection<ModifierMask>();
        private readonly Window _window;

        private ModifierMask _selectedModifier;
        public ModifierMask SelectedModifier
        {
            get => _selectedModifier;
            set
            {
                _selectedModifier = value;
                OnPropertyChanged();
            }
        }

        private string _keyText;
        public string KeyText
        {
            get => _keyText;
            set
            {
                // Перевіряємо, чи введено лише одну літеру
                if (!string.IsNullOrEmpty(value) &&  !Regex.IsMatch(value, @"^[a-zA-Z0-9]$"))
                {
                    ErrorMessageVisible = true;
                    ErrorMessage = "Дозволяється лише одна англійська літера або одна цифра";

                    return; // Не дозволяємо змінювати значення
                }

                _keyText = value;
                ErrorMessageVisible = false;
               
                OnPropertyChanged(nameof(KeyText));
            }
        }

        private bool _runMultipleInstances;
        public bool RunMultipleInstances 
        {
            get => _runMultipleInstances;
            set 
            {
                _runMultipleInstances = value;
                OnPropertyChanged();
            }
        }

        private bool _errorMassageVisible;

        public bool ErrorMessageVisible
        {

            get => _errorMassageVisible;
            set
            {
                _errorMassageVisible = value;
                OnPropertyChanged(nameof(ErrorMessageVisible));
            }
        }
        private string _errorMassage;
        public string ErrorMessage 
        {
        
            get => _errorMassage;
            set { 
                _errorMassage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        } 
        
        private string _programPath;
        public string ProgramPath
        {
            get => _programPath;
            set
            {
                _programPath = value;
                OnPropertyChanged();
            }
        }

        public Hotkey Result { get; private set; }
        public event EventHandler<Hotkey> HotkeySaved;
    
        public ICommand SelectProgramCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddEditViewModel(Window window, Hotkey hotkeyToEdit = null)
        {
            _window = window;
            LoadModifiers(); 
            SelectProgramCommand = new RelayCommand(SelectProgramAsync);
            SaveCommand = new RelayCommand(Save); 
            CancelCommand = new RelayCommand(Cancel); 

            if (hotkeyToEdit != null)
            {
                SelectedModifier = hotkeyToEdit.Modifier;
                KeyText = hotkeyToEdit.Key.ToString().Substring(2); 
                ProgramPath = hotkeyToEdit.ProgramPath;
                RunMultipleInstances = hotkeyToEdit.RunMultipleInstances;
            }
        }


        private void LoadModifiers()
        {
            Modifiers.Add(ModifierMask.None);
            Modifiers.Add(ModifierMask.LeftShift);
            Modifiers.Add(ModifierMask.RightShift);
            Modifiers.Add(ModifierMask.LeftCtrl);
            Modifiers.Add(ModifierMask.RightCtrl);
            Modifiers.Add(ModifierMask.LeftAlt);
            Modifiers.Add(ModifierMask.RightAlt);

            SelectedModifier = ModifierMask.None;
        }


        private async Task SelectProgramAsync()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Оберіть програму для запуску",
                AllowMultiple = false
            };
            dialog.Filters.Add(new FileDialogFilter
            {
                Name = "Виконувані файли",
                Extensions = OperatingSystem.IsWindows()
                    ? new() { "exe" }
                    : new() { "bin", "" }
            });
            var window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            var result = await dialog.ShowAsync(window);
            if (result != null && result.Length > 0)
            {
                ProgramPath = result[0];
            }
        }

        public KeyCode KeyCodeFromChar()
        {
            if (string.IsNullOrEmpty(KeyText) || KeyText.Length != 1)
            {
                return KeyCode.VcUndefined; 
            }

            char c = char.ToUpper(KeyText[0]); 

            // Перетворюємо букви A-Z у відповідні KeyCode
            if (c >= 'A' && c <= 'Z')
            {
                return (KeyCode)((int)KeyCode.VcA + (c - 'A')); // Зміщення від VcA
            }

            // Перетворюємо цифри 0-9 у відповідні KeyCode
            if (c >= '0' && c <= '9')
            {
                return (KeyCode)((int)KeyCode.Vc0 + (c - '0')); // Зміщення від Vc0
            }

            // Повертаємо Unknown для непідтримуваних символів
            return KeyCode.VcUndefined;
        }

        
        private void Save()
        {
            if (!string.IsNullOrEmpty(KeyText))
            {
                KeyCode key = KeyCodeFromChar();
                if (string.IsNullOrEmpty(ProgramPath))
                {
                  Result = null;
                  HotkeySaved?.Invoke(this, null); 
                  ErrorMessageVisible = true;
                  ErrorMessage = $"Оберіть програму";
                  return;
                    
                }
                if (SelectedModifier == ModifierMask.None)
                {
                  Result = null;
                  HotkeySaved?.Invoke(this, null); 
                  ErrorMessageVisible = true;
                  ErrorMessage = $"Оберіть модифікатор";
                  return;
                }
                Result = new Hotkey(0, SelectedModifier, key, ProgramPath, RunMultipleInstances);
                HotkeySaved?.Invoke(this, Result); 
                _window.Close();
            }
            else {
                ErrorMessageVisible = true;
                ErrorMessage = $"Спочатку введи букву";
            }
        }

        
        private void Cancel()
        {
            Result = null;
            HotkeySaved?.Invoke(this, null);
            _window.Close();

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       
        
    }
}