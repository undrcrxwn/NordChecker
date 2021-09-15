using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NordChecker.Commands;
using NordChecker.Models;
using NordChecker.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NordChecker.ViewModels
{
    public class ExportWindowViewModel : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        public IAppSettings Settings { get; set; }

        private bool _IsWindowVisible = true;
        public bool IsWindowVisible
        {
            get => _IsWindowVisible;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsWindowVisible, value, PropertyChanged);
        }

        private string _Path;
        public string Path
        {
            get => _Path;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Path, value, PropertyChanged);
        }

        private bool _ArePremiumIncluded = true;
        public bool ArePremiumIncluded
        {
            get => _ArePremiumIncluded;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArePremiumIncluded, value, PropertyChanged);
        }

        private bool _AreFreeIncluded = true;
        public bool AreFreeIncluded
        {
            get => _AreFreeIncluded;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreFreeIncluded, value, PropertyChanged);
        }

        private bool _AreInvalidIncluded = true;
        public bool AreInvalidIncluded
        {
            get => _AreInvalidIncluded;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreInvalidIncluded, value, PropertyChanged);
        }

        private bool _AreUncheckedIncluded = true;
        public bool AreUncheckedIncluded
        {
            get => _AreUncheckedIncluded;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreUncheckedIncluded, value, PropertyChanged);
        }

        private bool _IsOperationConfirmed = false;
        public bool IsOperationConfirmed
        {
            get => _IsOperationConfirmed;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsOperationConfirmed, value, PropertyChanged);
        }

        #endregion

        public ICommand ChoosePathCommand { get; }

        private bool CanExecuteChoosePathCommand(object parameter) => true;

        private void OnChoosePathCommandExecuted(object parameter)
        {
            Log.Information("OnChoosePathCommandExecuted");

            Task.Run(() =>
            {
                var dialog = new CommonOpenFileDialog() { IsFolderPicker = true };
                IsWindowVisible = false;
                CommonFileDialogResult result = Application.Current.Dispatcher.Invoke(dialog.ShowDialog);
                IsWindowVisible = true;
                if (result != CommonFileDialogResult.Ok) return;
                
                Path = dialog.FileName;
            });
        }

        public ExportWindowViewModel(IAppSettings settings)
        {
            Settings = settings;
            ChoosePathCommand = new LambdaCommand(OnChoosePathCommandExecuted, CanExecuteChoosePathCommand);
        }
    }
}
