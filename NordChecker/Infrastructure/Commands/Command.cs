using System;
using System.Windows.Input;
using Serilog;

namespace NordChecker.Infrastructure.Commands
{
    public class LoggedCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private readonly Action<object> _Execute;
        private readonly Func<object, bool> _CanExecute;
        private readonly string _Name;

        public LoggedCommand(string name, Action<object> execute, Func<object, bool> canExecute = null)
        {
            _Name = name;
            _Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _CanExecute = canExecute;
        }
        
        public LoggedCommand(Action<object> execute, Func<object, bool> canExecute = null)
            : this($"unknown {nameof(LoggedCommand)}", execute, canExecute) {}
        
        public bool CanExecute(object parameter) =>
            _CanExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter = null)
        {
            Log.Information("Executing {0}", _Name);
            _Execute(parameter);
        }
    }
}
