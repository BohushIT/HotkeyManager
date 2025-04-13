using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HotkeyManager.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object, Task> _executeAsync;
        private readonly Func<object, bool> _canExecute;
        private readonly bool _preventConcurrentExecution;
        private bool _isExecuting;
        private Action<object> executeToggleAutoStart;

        public RelayCommand(Action<object> executeToggleAutoStart)
        {
            this.executeToggleAutoStart = executeToggleAutoStart;
        }

        public RelayCommand(Func<Task> executeAsync, Func<bool> canExecute = null, bool preventConcurrentExecution = false)
            : this(p => executeAsync(), p => canExecute?.Invoke() ?? true, preventConcurrentExecution)
        {
        }
        public RelayCommand(Action execute, Func<bool> canExecute = null, bool preventConcurrentExecution = false)
            : this(p => { execute(); return Task.CompletedTask; }, p => canExecute?.Invoke() ?? true, preventConcurrentExecution)
        {
        }

        private RelayCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute, bool preventConcurrentExecution)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
            _preventConcurrentExecution = preventConcurrentExecution;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return (!_preventConcurrentExecution || !_isExecuting) && (_canExecute?.Invoke(parameter) ?? true);
        }

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            if (_preventConcurrentExecution)
            {
                _isExecuting = true;
            }

            try
            {
                await _executeAsync(parameter);
            }
            finally
            {
                if (_preventConcurrentExecution)
                {
                    _isExecuting = false;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
