using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using Serilog;

namespace NordChecker.ViewModels
{
    public partial class ImportProxiesPageViewModel
    {
        #region ChoosePathCommand

        public ICommand ChoosePathCommand
        {
            get;
        }

        private void OnChoosePathCommandExecuted()
        {
            Log.Information("OnChoosePathCommandExecuted");

            Task.Run(() =>
            {
                var dialog = new OpenFileDialog();
                dialog.DefaultExt = ".txt";
                dialog.Filter = "NordVPN Proxy List|*.txt|Все файлы|*.*";
                if (dialog.ShowDialog() != true) return;

                FilePath = dialog.FileName;
            });
        }

        #endregion
    }
}
