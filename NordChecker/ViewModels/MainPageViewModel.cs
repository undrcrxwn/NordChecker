using Leaf.xNet;
using Microsoft.Win32;
using NordChecker.Models;
using NordChecker.Shared;
using NordChecker.Views;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using NordChecker.Models.Settings;
using NordChecker.Infrastructure;
using NordChecker.Services;
using NordChecker.Services.Checker;
using NordChecker.Services.Threading;
using NordChecker.Shared.Collections;
using NordChecker.Commands;

namespace NordChecker.ViewModels
{
    public partial class MainPageViewModel : IPageViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private NavigationService navigationService;

        public IChecker Checker;
        public Parser ComboParser;
        private ThreadDistributor<Account> distributor;
        private MasterTokenSource tokenSource = new MasterTokenSource();
        private Stopwatch progressWatch = new Stopwatch();

        #region Properties

        private string _Title;
        public string Title
        {
            get => _Title;
            protected set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Title, value, PropertyChanged);
        }

        private Account _SelectedAccount;
        public Account SelectedAccount
        {
            get => _SelectedAccount;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _SelectedAccount, value, PropertyChanged);
        }

        public AppSettings AppSettings { get; set; }
        public ExportSettings ExportSettings { get; set; }
        public ProxiesViewModel ProxiesViewModel { get; set; }

        private ObservableCollection<Account> _Accounts;
        public ObservableCollection<Account> Accounts
        {
            get => _Accounts;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Accounts, value, PropertyChanged);
        }

        private ComboStats _ComboStats = new ComboStats();
        public ComboStats ComboStats
        {
            get => _ComboStats;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ComboStats, value, PropertyChanged);
        }

        private ObservableDictionary<AccountState, Arc> _ComboArcs;
        public ObservableDictionary<AccountState, Arc> ComboArcs
        {
            get => _ComboArcs;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ComboArcs, value, PropertyChanged);
        }

        public bool IsPipelineIdle { get => PipelineState == PipelineState.Idle; }
        public bool IsPipelinePaused { get => PipelineState == PipelineState.Paused; }
        public bool IsPipelineWorking { get => PipelineState == PipelineState.Working; }

        private PipelineState _PipelineState = PipelineState.Idle;
        public PipelineState PipelineState
        {
            get => _PipelineState;
            set
            {
                INotifyPropertyChangedAdvanced @this = this;
                @this.Set(ref _PipelineState, value, PropertyChanged, LogEventLevel.Information);
                @this.OnPropertyChanged(PropertyChanged, nameof(IsPipelineIdle));
                @this.OnPropertyChanged(PropertyChanged, nameof(IsPipelinePaused));
                @this.OnPropertyChanged(PropertyChanged, nameof(IsPipelineWorking));
            }
        }

        private bool _IsGreetingVisible = true;
        public bool IsGreetingVisible
        {
            get => _IsGreetingVisible;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsGreetingVisible, value, PropertyChanged);
        }

        #endregion
        
        public void RefreshComboArcs()
        {
            int loaded = Math.Max(1, Accounts.Count);
            Dictionary<AccountState, float> shares =
                ComboStats.ByState.ToDictionary(p => p.Key, p => (float)p.Value / loaded);

            float margin = 6;
            float pivot = margin / 2;
            float maxPossibleAngle = 360 - (shares.Values.Count(v => v > 0) * margin);
            foreach (var (state, share) in shares)
            {
                switch (share)
                {
                    case 0:
                        ComboArcs[state].StartAngle = 0;
                        ComboArcs[state].EndAngle = 1;
                        ComboArcs[state].Visibility = Visibility.Hidden;
                        break;
                    case 1:
                        ComboArcs[state].StartAngle = 0;
                        ComboArcs[state].EndAngle = 360;
                        ComboArcs[state].Visibility = Visibility.Visible;
                        break;
                    default:
                        ComboArcs[state].StartAngle = pivot;
                        pivot += share * maxPossibleAngle;
                        ComboArcs[state].EndAngle = pivot;
                        pivot += margin;
                        ComboArcs[state].Visibility = Visibility.Visible;
                        break;
                }
            }

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Application.Current.MainWindow.TaskbarItemInfo ??= new System.Windows.Shell.TaskbarItemInfo();
                if (ComboStats.ByState[AccountState.Unchecked] + ComboStats.ByState[AccountState.Reserved] > 0)
                {
                    Application.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    Application.Current.MainWindow.TaskbarItemInfo.ProgressValue
                        = 1 - shares[AccountState.Unchecked] - shares[AccountState.Reserved];
                }
                else
                {
                    Application.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    Application.Current.MainWindow.TaskbarItemInfo.ProgressValue = 0;
                }
            });
        }

        private void AccountsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsGreetingVisible = Accounts.Count == 0;
            if (Accounts.Count == 0)
            {
                StopCommand.Execute(null);
                ComboStats.Clear();
            }
        }

        private void UpdatePageDescription()
        {
            int checkedCount = Accounts.Count
                - ComboStats.ByState[AccountState.Unchecked]
                - ComboStats.ByState[AccountState.Reserved];
            float percentageChecked = checkedCount
                / Math.Max(Accounts.Count, 1.0f) * 100;
            StringBuilder builder = new StringBuilder();
            builder.Append($"{percentageChecked:0}%");

            if (PipelineState != PipelineState.Idle)
            {
                TimeSpan elapsed = progressWatch.Elapsed;
                builder.Append($" ({elapsed.ToShortDurationString()} затрачено");

                if (percentageChecked is > 0 and < 100)
                {
                    TimeSpan left = elapsed * (100 - percentageChecked) / percentageChecked;
                    builder.Append($", {left.ToShortDurationString()} осталось");
                }
                builder.Append(")");
            }

            Title = builder.ToString();
        }

        public MainPageViewModel(
            IChecker checker,
            ObservableCollection<Account> accounts,
            NavigationService navigationService,
            AppSettings appSettings,
            ExportSettings exportSettings,
            ProxiesViewModel proxiesViewModel)
        {
            Checker = checker;
            Accounts = accounts;
            this.navigationService = navigationService;
            AppSettings = appSettings;
            ExportSettings = exportSettings;
            ProxiesViewModel = proxiesViewModel;

            ComboParser = new Parser(AppSettings.ComboRegexMask);

            ComboStats.PropertyChanged += (sender, e) =>
                (this as INotifyPropertyChangedAdvanced)
                .OnPropertyChanged(PropertyChanged, nameof(ComboStats));

            _ComboArcs = new ObservableDictionary<AccountState, Arc>();
            foreach (AccountState key in Enum.GetValues(typeof(AccountState)))
                _ComboArcs.Add(key, new Arc(0, 1, Visibility.Hidden));

            ComboArcs.CollectionChanged += (sender, e) =>
                (this as INotifyPropertyChangedAdvanced)
                .OnPropertyChanged(PropertyChanged, nameof(ComboArcs));

            ComboStats.PropertyChanged += (sender, e) => RefreshComboArcs();

            Accounts.CollectionChanged += AccountsCollectionChangedHandler;

            distributor = new ThreadDistributor<Account>(
                AppSettings.ThreadCount,
                Accounts,
                account =>
                {
                    if (account.State == AccountState.Unchecked)
                    {
                        account.MasterToken = tokenSource.MakeToken();
                        account.State = AccountState.Reserved;
                        lock (ComboStats.ByState)
                        {
                            ComboStats.ByState[AccountState.Unchecked]--;
                            ComboStats.ByState[AccountState.Reserved]++;
                        }
                        return true;
                    }
                    return false;
                },
                checker.ProcessAccount);

            distributor.TaskCompleted += (sender, account) =>
            {
                int uncompletedCount;
                lock (ComboStats.ByState)
                {
                    ComboStats.ByState[AccountState.Reserved]--;
                    ComboStats.ByState[account.State]++;

                    uncompletedCount = ComboStats.ByState[AccountState.Unchecked]
                        + ComboStats.ByState[AccountState.Reserved];
                }
                
                if (uncompletedCount == 0)
                    PauseCommand.Execute(null);
            };
            
            #region Commands

            StartCommand = new RelayCommand(nameof(StartCommand), OnStartCommandExecuted, CanExecuteStartCommand);
            PauseCommand = new RelayCommand(nameof(PauseCommand), OnPauseCommandExecuted, CanExecutePauseCommand);
            ContinueCommand = new RelayCommand(nameof(ContinueCommand), OnContinueCommandExecuted, CanExecuteContinueCommand);
            StopCommand = new RelayCommand(nameof(StopCommand), OnStopCommandExecuted, CanExecuteStopCommand);

            LoadCombosCommand = new RelayCommand(nameof(LoadCombosCommand), OnLoadCombosCommandExecuted);
            ClearCombosCommand = new RelayCommand(nameof(ClearCombosCommand), OnClearCombosCommandExecuted, CanExecuteClearCombosCommand);
            StopAndClearCombosCommand = new RelayCommand(nameof(StopAndClearCombosCommand), OnStopAndClearCombosCommandExecuted, CanExecuteStopAndClearCombosCommand);
            LoadProxiesCommand = new RelayCommand(nameof(LoadProxiesCommand), OnLoadProxiesCommandExecuted);
            ExportCommand = new RelayCommand(nameof(ExportCommand), OnExportCommandExecuted, CanExecuteExportCommand);

            CopyAccountCredentialsCommand = new RelayCommand(nameof(CopyAccountCredentialsCommand), OnCopyAccountCredentialsCommandExecuted);
            RemoveAccountCommand = new RelayCommand(nameof(RemoveAccountCommand), OnRemoveAccountCommandExecuted);

            ContactAuthorCommand = new RelayCommand(nameof(ContactAuthorCommand), OnContactAuthorCommandExecuted);

            #endregion

            AppSettings.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(AppSettings.ComboRegexMask):
                        ComboParser.RegexMask = AppSettings.ComboRegexMask;
                        break;
                    case nameof(AppSettings.ThreadCount):
                        distributor.ThreadCount = AppSettings.ThreadCount;
                        break;
                }
            };

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(PipelineState))
                    UpdatePageDescription();
            };

            Task.Run(async () =>
            {
                while (true)
                {
                    ProxiesViewModel.Refresh();
                    UpdatePageDescription();
                    await Task.Delay(1000);
                }
            });
        }
    }
}
