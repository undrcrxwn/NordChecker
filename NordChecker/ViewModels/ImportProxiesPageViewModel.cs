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

        private string _FilePath;
        public string FilePath
        {
            get => _FilePath;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _FilePath, value, PropertyChanged);
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
