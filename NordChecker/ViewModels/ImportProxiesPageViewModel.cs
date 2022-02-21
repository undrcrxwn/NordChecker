using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Leaf.xNet;
using NordChecker.Infrastructure;
using NordChecker.Models;
using NordChecker.Models.Settings;
using NordChecker.Models.Stats;
using NordChecker.Services;
using NordChecker.Shared;
using NordChecker.Shared.Collections;
using Prism.Commands;
using Prism.Mvvm;

namespace NordChecker.ViewModels
{
    public partial class ImportProxiesPageViewModel : INotifyPropertyChanged, IPageViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Title => "Импорт прокси";

        public AppSettings AppSettings { get; set; }
        public ImportSettings ImportSettings { get; set; }
        public ImportSettings ImportSettingsDraft { get; set; }
        public NavigationService NavigationService { get; set; }
        public ProxyParser ProxyParser { get; set; }
        public ProxyStats ProxyStats { get; set; }
        public Cyclic<Proxy> Proxies { get; set; }

        private string _FilePath;
        public string FilePath
        {
            get => _FilePath;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _FilePath, value, PropertyChanged);
        }

        public ImportProxiesPageViewModel(
            AppSettings appSettings,
            ImportSettings importSettings,
            NavigationService navigationService,
            ProxyParser proxyParser,
            ProxyStats proxyStats,
            Cyclic<Proxy> proxies)
        {
            AppSettings = appSettings;
            ImportSettings = importSettings;
            ImportSettingsDraft = ImportSettings.Clone();
            NavigationService = navigationService;
            ProxyParser = proxyParser;
            ProxyStats = proxyStats;
            Proxies = proxies;

            BindingHelper.BindOneWay(
                ImportSettings, nameof(ImportSettingsDraft.ProxyRegexMask),
                ProxyParser, nameof(ProxyParser.RegexPattern));

            ChoosePathCommand = new DelegateCommand(OnChoosePathCommandExecuted);
            ProceedCommand = new DelegateCommand(OnProceedCommandExecuted);
            CancelCommand = new DelegateCommand(OnCancelCommandExecuted);
        }
    }
}
