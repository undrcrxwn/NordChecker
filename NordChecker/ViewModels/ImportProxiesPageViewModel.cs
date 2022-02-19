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

        public Wrapped<AppSettings> AppSettingsWrapped { get; set; }
        public Wrapped<ImportSettings> ImportSettingsWrapped { get; set; }
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
            Wrapped<AppSettings> appSettingsWrapped,
            Wrapped<ImportSettings> importSettingsWrapped,
            NavigationService navigationService,
            ProxyParser proxyParser,
            ProxyStats proxyStats,
            Cyclic<Proxy> proxies)
        {
            AppSettingsWrapped = appSettingsWrapped;
            ImportSettingsWrapped = importSettingsWrapped;
            NavigationService = navigationService;
            ProxyParser = proxyParser;
            ProxyStats = proxyStats;
            Proxies = proxies;

            PropertyChangedEventHandler lastBindingHandler = null;
            ImportSettingsWrapped.ForEach(instance =>
            {
                // Unbind from deprecated instance's property
                if (ImportSettingsWrapped.HasValue && lastBindingHandler != null)
                    BindingHelper.UnbindOneWay(
                        ImportSettingsWrapped.Instance, lastBindingHandler);

                // Bind to new instance's property
                lastBindingHandler = BindingHelper.BindOneWay(
                    instance, nameof(ImportSettings.ProxyRegexMask),
                    ProxyParser, nameof(ProxyParser.RegexPattern));
            });

            ChoosePathCommand = new DelegateCommand(OnChoosePathCommandExecuted);
            ProceedCommand = new DelegateCommand(OnProceedCommandExecuted);
            CancelCommand = new DelegateCommand(OnCancelCommandExecuted);
        }
    }
}
