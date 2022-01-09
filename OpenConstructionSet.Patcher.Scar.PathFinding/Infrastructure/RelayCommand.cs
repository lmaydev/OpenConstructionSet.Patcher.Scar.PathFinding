using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure
{
    public class RelayCommand : ICommand
    {
        private Action execute;
        private Func<bool>? canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private Action<T?> execute;
        private Func<T?, bool>? canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter is not T value)
            {
                return false;
            }

            return canExecute?.Invoke(value) ?? true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is not T value)
            {
                return;
            }

            execute(value);
        }
    }
}
