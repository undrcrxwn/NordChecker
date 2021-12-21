using System;
using System.Runtime.CompilerServices;
using Serilog;

namespace NordChecker.Commands
{
    public class RelayCommand : LoggedCommand
    {
        public RelayCommand(string name, Action<object> execute, Func<object, bool> canExecute = null)
            : base(name, execute, canExecute) { }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
            : base($"Unknown {nameof(RelayCommand)}", execute, canExecute) { }
    }
}
