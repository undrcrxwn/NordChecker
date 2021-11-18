using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NordChecker.Infrastructure;
using Serilog;

namespace NordChecker.ViewModels
{
    public partial class MainWindowViewModel
    {
        #region OpenFromTrayCommand

        public ICommand OpenFromTrayCommand { get; }

        private bool CanExecuteOpenFromTrayCommand(object parameter) => true;

        private void OnOpenFromTrayCommandExecuted(object parameter)
        {
            Log.Information("OnOpenFromTrayCommandExecuted");
            WindowVisibility = Visibility.Visible;
        }

        #endregion
    }
}
