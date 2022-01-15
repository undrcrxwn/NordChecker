using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leaf.xNet;
using NordChecker.Infrastructure;
using NordChecker.Models.Settings;
using Prism.Commands;
using Prism.Mvvm;

namespace NordChecker.ViewModels
{
    public partial class ImportProxiesPageViewModel : BindableBase, IPageViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        public string Title => "Импорт прокси";

        public Wrapped<AppSettings> AppSettingsWrapped { get; set; }
        public Wrapped<ImportSettings> ImportSettingsWrapped { get; set; }

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

        public ImportProxiesPageViewModel(
            Wrapped<AppSettings> appSettingsWrapped,
            Wrapped<ImportSettings> importSettingsWrapped)
        {
            AppSettingsWrapped = appSettingsWrapped;
            ImportSettingsWrapped = importSettingsWrapped;

            ChoosePathCommand = new DelegateCommand(OnChoosePathCommandExecuted);
        }
    }
}
