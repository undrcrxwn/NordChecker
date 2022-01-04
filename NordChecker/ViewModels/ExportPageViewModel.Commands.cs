using Microsoft.WindowsAPICodePack.Dialogs;
using NordChecker.Views;
using Serilog;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NordChecker.ViewModels
{
    public partial class ExportPageViewModel
    {
        #region ChoosePathCommand

        public ICommand ChoosePathCommand { get; }

        private bool CanExecuteChoosePathCommand(object parameter) => true;

        private void OnChoosePathCommandExecuted(object parameter)
        {
            Log.Information("OnChoosePathCommandExecuted");

            Task.Run(() =>
            {
                var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
                CommonFileDialogResult result = Application.Current.Dispatcher.Invoke(dialog.ShowDialog);
                if (result != CommonFileDialogResult.Ok) return;

                OutputDirectoryPath = dialog.FileName;
            });
        }

        #endregion

        #region NavigateHomeCommand

        public ICommand NavigateHomeCommand { get; }

        private bool CanExecuteNavigateHomeCommand(object parameter) => true;

        private void OnNavigateHomeCommandExecuted(object parameter)
        {
            Log.Information("OnNavigateHomeCommandExecuted");
            navigationService.Navigate<MainPage>();
        }

        #endregion
    }
}
