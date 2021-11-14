using Leaf.xNet;
using Microsoft.Win32;
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
using System.Windows.Input;
using NordChecker.Infrastructure;

namespace NordChecker.ViewModels
{
    public class LoadProxiesWindowViewModel : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        private bool _IsWindowVisible = true;
        public bool IsWindowVisible
        {
            get => _IsWindowVisible;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsWindowVisible, value, PropertyChanged);
        }

        private ProxyType _ProxyType = ProxyType.Socks4;
        public ProxyType ProxyType
        {
            get => _ProxyType;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ProxyType, value, PropertyChanged);
        }

        private string _Path;
        public string Path
        {
            get => _Path;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Path, value, PropertyChanged);
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
                var dialog = new OpenFileDialog();
                dialog.DefaultExt = ".txt";
                dialog.Filter = "NordVPN Proxy List|*.txt|Все файлы|*.*";
                IsWindowVisible = false;
                if (dialog.ShowDialog() != true) return;
                IsWindowVisible = true;

                Path = dialog.FileName;
            });
        }

        public LoadProxiesWindowViewModel()
        {
            ChoosePathCommand = new LambdaCommand(OnChoosePathCommandExecuted, CanExecuteChoosePathCommand);
        }
    }
}
