using Leaf.xNet;
using Microsoft.Win32;
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
using Prism.Commands;

namespace NordChecker.ViewModels
{
    public partial class LoadProxiesWindowViewModel : INotifyPropertyChangedAdvanced
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

        private bool _IsOperationConfirmed;
        public bool IsOperationConfirmed
        {
            get => _IsOperationConfirmed;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsOperationConfirmed, value, PropertyChanged);
        }

        #endregion
        
        public LoadProxiesWindowViewModel()
        {
            ChoosePathCommand = new DelegateCommand(OnChoosePathCommandExecuted);
        }
    }
}
