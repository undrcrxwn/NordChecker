using Leaf.xNet;
using Microsoft.Win32;
using NordChecker.Commands;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NordChecker.ViewModels
{
    public class MainPageViewModel : IPageViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private INavigationService navigationService;

        private ThreadDistributor<Account> distributor;
        private MasterTokenSource tokenSource = new MasterTokenSource();
        private Checker checker = new Checker(7000);
        private Stopwatch progressWatch = new Stopwatch();

        #region Properties

        private string _Title;
        public string Title
        {
            get => _Title;
            private set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Title, value, PropertyChanged);
        }

        private string _Description;
        public string Description
        {
            get => _Description;
            private set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Description, value, PropertyChanged);
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

        private ObservableCollection<Account> _Accounts = new ObservableCollection<Account>();
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

        private ObservableDictionary<AccountState, ArcViewModel> _ComboArcs;
        public ObservableDictionary<AccountState, ArcViewModel> ComboArcs
        {
            get => _ComboArcs;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ComboArcs, value, PropertyChanged);
        }

        private ProxyDispenserViewModel _ProxyDispenser = new ProxyDispenserViewModel();
        public ProxyDispenserViewModel ProxyDispenser
        {
            get => _ProxyDispenser;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ProxyDispenser, value, PropertyChanged);
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

        #region Commands

        #region StartCommand

        public ICommand StartCommand { get; }

        private bool CanExecuteStartCommand(object parameter) => PipelineState == PipelineState.Idle;

        private void OnStartCommandExecuted(object parameter)
        {
            Log.Information("OnStartCommandExecuted");
            PipelineState = PipelineState.Working;

            progressWatch.Restart();
            new Thread(() =>
            {
                distributor = new ThreadDistributor<Account>(
                    AppSettings.ThreadCount,
                    Accounts,
                    account =>
                    {
                        if (account.State == AccountState.Unchecked)
                        {
                            account.MasterToken = tokenSource.MakeToken();
                            account.State = AccountState.Reserved;
                            lock (ComboStats)
                            {
                                ComboStats.ByState[AccountState.Unchecked]--;
                                ComboStats.ByState[AccountState.Reserved]++;
                            }
                            return true;
                        }
                        return false;
                    },
                    checker.ProcessAccount,
                    tokenSource.MakeToken());

                distributor.OnTaskCompleted += (sender, account) =>
                {
                    lock (ComboStats)
                    {
                        ComboStats.ByState[AccountState.Reserved]--;
                        ComboStats.ByState[account.State]++;
                    }

                    int uncompletedCount = ComboStats.ByState[AccountState.Unchecked]
                        + ComboStats.ByState[AccountState.Reserved];
                    if (uncompletedCount == 0)
                        PauseCommand.Execute(null);
                };
            })
            { IsBackground = true }.Start();
        }

        #endregion

        #region PauseCommand

        public ICommand PauseCommand { get; }

        private bool CanExecutePauseCommand(object parameter) => PipelineState == PipelineState.Working;

        private void OnPauseCommandExecuted(object parameter)
        {
            Log.Information("OnPauseCommandExecuted");
            progressWatch.Stop();

            PipelineState = PipelineState.Paused;
            tokenSource.Pause();
        }

        #endregion

        #region ContinueCommand

        public ICommand ContinueCommand { get; }

        private bool CanExecuteContinueCommand(object parameter) => PipelineState == PipelineState.Paused;

        private void OnContinueCommandExecuted(object parameter)
        {
            Log.Information("OnContinueCommandExecuted");
            progressWatch.Start();

            PipelineState = PipelineState.Working;
            tokenSource.Continue();
        }

        #endregion

        #region StopCommand

        public ICommand StopCommand { get; }

        private bool CanExecuteStopCommand(object parameter) => PipelineState != PipelineState.Idle;

        private void OnStopCommandExecuted(object parameter)
        {
            Log.Information("OnStopCommandExecuted");
            progressWatch.Stop();

            PipelineState = PipelineState.Idle;
            tokenSource.Cancel();
        }

        #endregion

        #region LoadCombosCommand

        public ICommand LoadCombosCommand { get; }

        private bool CanExecuteLoadCombosCommand(object parameter) => true;

        private void OnLoadCombosCommandExecuted(object parameter)
        {
            Log.Information("OnLoadCombosCommandExecuted");

            Task.Run(() =>
            {
                var dialog = new OpenFileDialog();
                dialog.DefaultExt = ".txt";
                dialog.Filter = "NordVPN Combo List|*.txt|Все файлы|*.*";
                if (dialog.ShowDialog() != true) return;

                Task.Factory.StartNew(() =>
                {
                    Log.Information("Reading combos from {file}", dialog.FileName);
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    string line;
                    List<Account> cache = new List<Account>();
                    using (StreamReader reader = new StreamReader(File.OpenRead(dialog.FileName)))
                    {
                        while ((line = reader.ReadLine()) != null)
                        {
                            Account account;
                            try
                            {
                                account = Parser.Parse(line);
                            }
                            catch
                            {
                                ComboStats.MismatchedCount++;
                                Log.Debug("Line \"{line}\" has been skipped as mismatched", line);
                                continue;
                            }

                            if (AppSettings.AreComboDuplicatesSkipped)
                            {
                                if (Accounts.Any(a => a.Credentials == account.Credentials) ||
                                    cache.Any(a => a.Credentials == account.Credentials))
                                {
                                    ComboStats.DuplicatesCount++;
                                    Log.Debug("Account {credentials} has been skipped as duplicate", account.Credentials);
                                    continue;
                                }
                            }

                            cache.Add(account);
                            Log.Debug("Account {credentials} has been added to the cache", account.Credentials);
                        }
                    }

                    watch.Stop();
                    Log.Information("{total} accounts have been extracted from {file} in {elapsed}ms",
                        cache.Count, dialog.FileName, watch.ElapsedMilliseconds);

                    DispatcherExtensions.BeginInvoke(Application.Current.Dispatcher, () =>
                     {
                         foreach (Account account in cache)
                             Accounts.Add(account);
                     });
                });
            });
        }

        #endregion

        #region ClearCombosCommand

        public ICommand ClearCombosCommand { get; }

        private bool CanExecuteClearCombosCommand(object parameter) => PipelineState != PipelineState.Working;

        private void OnClearCombosCommandExecuted(object parameter)
        {
            Log.Information("OnClearCombosCommandExecuted");

            Accounts.Clear();
            ComboStats.Clear();
        }

        #endregion

        #region StopAndClearCombosCommand

        public ICommand StopAndClearCombosCommand { get; }

        private bool CanExecuteStopAndClearCombosCommand(object parameter)
            => StopCommand.CanExecute(null) && ClearCombosCommand.CanExecute(null);

        private void OnStopAndClearCombosCommandExecuted(object parameter)
        {
            Log.Information("OnStopAndClearCombosCommandExecuted");

            StopCommand.Execute(null);
            ClearCombosCommand.Execute(null);
        }

        #endregion

        #region LoadProxiesCommand

        public ICommand LoadProxiesCommand { get; }

        private bool CanExecuteLoadProxiesCommand(object parameter) => true;

        private void OnLoadProxiesCommandExecuted(object parameter)
        {
            Log.Information("OnLoadProxiesCommandExecuted");

            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Window window = new LoadProxiesWindow();
                    window.Owner = Application.Current.MainWindow;
                    window.ShowDialog();

                    var result = window.DataContext as LoadProxiesWindowViewModel;
                    if (!result.IsOperationConfirmed) return;

                    Task.Factory.StartNew(() =>
                    {
                        Log.Information("Reading {type} proxies from {file}", result.ProxyType, result.Path);
                        Stopwatch watch = new Stopwatch();
                        watch.Start();

                        string line;
                        using (StreamReader reader = new StreamReader(File.OpenRead(result.Path)))
                        {
                            lock (ProxyDispenser.Proxies)
                            {
                                Log.Warning("Locked {0}", nameof(ProxyDispenser.Proxies));
                                while ((line = reader.ReadLine()) != null)
                                {
                                    ProxyClient client;
                                    try
                                    {
                                        var types = Enum.GetValues(typeof(ProxyType));
                                        var type = (ProxyType)types.GetValue(new Random().Next(0, types.Length));
                                        //client = ProxyClient.Parse(result.ProxyType, line);
                                        client = ProxyClient.Parse(type, line);
                                    }
                                    catch (Exception e)
                                    {
                                        ProxyDispenser.MismatchedCount++;
                                        Log.Error(e, "Line \"{line}\" has been skipped as mismatched", line);
                                        continue;
                                    }

                                    if (AppSettings.AreProxyDuplicatesSkipped)
                                    {
                                        if (ProxyDispenser.Proxies.Any(p => p.Client.ToString() == client.ToString()))
                                        {
                                            ProxyDispenser.DuplicatesCount++;
                                            Log.Debug("Proxy {proxy} has been skipped as duplicate", client);
                                            continue;
                                        }
                                    }

                                    Proxy proxy = new Proxy(client);

                                    var states = Enum.GetValues(typeof(ProxyState));
                                    proxy.State = (ProxyState)states.GetValue(new Random().Next(0, states.Length));

                                    ProxyDispenser.Proxies.Add(proxy);
                                    ProxyDispenser.LoadedCount++;
                                    Log.Debug("Proxy {proxy} has been added to the dispenser", client);
                                }
                            }
                        }

                        watch.Stop();
                        Log.Information("{total} proxies have been extracted from {file} in {elapsed}ms",
                            ProxyDispenser.Proxies.Count, result.Path, watch.ElapsedMilliseconds);
                    });
                });
            });
        }

        #endregion

        #region ExportCommand

        public ICommand ExportCommand { get; }

        private bool CanExecuteExportCommand(object parameter)
            => PipelineState != PipelineState.Working
            && Accounts.Count > 0;

        private void OnExportCommandExecuted(object parameter)
        {
            Log.Information("OnExportCommandExecuted");
            navigationService.Navigate(new ExportPage(
                new ExportPageViewModel(navigationService, ExportSettings.Clone() as ExportSettings)));
        }

        #endregion

        #region CopyAccountCredentialsCommand

        public ICommand CopyAccountCredentialsCommand { get; }

        private bool CanExecuteCopyAccountCredentialsCommand(object parameter) => true;

        private void OnCopyAccountCredentialsCommandExecuted(object parameter)
        {
            Log.Information("OnCopyAccountCredentialsExecuted");
            var (mail, password) = SelectedAccount.Credentials;
            try
            {
                Clipboard.SetText($"{mail}:{password}");
                Log.Information("Clipboard text has been set to {0}", $"{mail}:{password}");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to write account credentials {0} to clipboard", (mail, password));
            }
        }

        #endregion

        #region RemoveAccountCommand

        public ICommand RemoveAccountCommand { get; }

        private bool CanExecuteRemoveAccountCommand(object parameter) => true;

        private void OnRemoveAccountCommandExecuted(object parameter)
        {
            Log.Information("OnRemoveAccountCommandExecuted");
            var account = SelectedAccount;
            account.MasterToken?.Cancel();
            Accounts.Remove(account);
            Log.Information("{0} has been removed", account.Credentials);
        }

        #endregion

        #region ContactAuthorCommand

        public ICommand ContactAuthorCommand { get; }

        private bool CanExecuteContactAuthorCommand(object parameter) => true;

        private void OnContactAuthorCommandExecuted(object parameter)
        {
            Log.Information("OnContactAuthorCommandExecuted");
            try
            {
                if (IsTelegramInstalled)
                    Process.Start("cmd", "/c start tg://resolve?domain=undrcrxwn");
                else
                    Process.Start("cmd", "/c start https://t.me/undrcrxwn");
            }
            catch (Exception e)
            {
                Log.Error(e, "Cannot open Telegram URL due to {exception}", e.GetType());
            }
        }

        private bool IsTelegramInstalled
        {
            get
            {
                string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKey))
                {
                    foreach (string subkeyName in key.GetSubKeyNames())
                    {
                        using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                        {
                            try
                            {
                                if (subkey.GetValue("DisplayName").ToString().ToLower().StartsWith("telegram"))
                                    return true;
                            }
                            catch { }
                        }
                    }
                }
                return false;
            }
        }

        #endregion

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
                if (share == 0)
                {
                    ComboArcs[state].StartAngle = 0;
                    ComboArcs[state].EndAngle = 1;
                    ComboArcs[state].Visibility = Visibility.Hidden;
                }
                else if (share == 1)
                {
                    ComboArcs[state].StartAngle = 0;
                    ComboArcs[state].EndAngle = 360;
                    ComboArcs[state].Visibility = Visibility.Visible;
                }
                else
                {
                    ComboArcs[state].StartAngle = pivot;
                    pivot += share * maxPossibleAngle;
                    ComboArcs[state].EndAngle = pivot;
                    pivot += margin;
                    ComboArcs[state].Visibility = Visibility.Visible;
                }
            }

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
            Application.Current.Dispatcher.InvokeAsync((Action)(() =>
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
            }));
        }

        public MainPageViewModel(INavigationService navigationService, AppSettings appSettings, ExportSettings exportSettings)
        {
            this.navigationService = navigationService;
            AppSettings = appSettings;
            ExportSettings = exportSettings;

            ComboStats.PropertyChanged += (sender, e) =>
                (this as INotifyPropertyChangedAdvanced)
                .OnPropertyChanged(PropertyChanged, nameof(ComboStats));

            _ComboArcs = new ObservableDictionary<AccountState, ArcViewModel>();
            foreach (AccountState key in Enum.GetValues(typeof(AccountState)))
                _ComboArcs.Add(key, new ArcViewModel(0, 1, Visibility.Hidden));

            ComboArcs.CollectionChanged += (sender, e) =>
                (this as INotifyPropertyChangedAdvanced)
                .OnPropertyChanged(PropertyChanged, nameof(ComboArcs));

            ComboStats.PropertyChanged += (sender, e) => RefreshComboArcs();

            Accounts.CollectionChanged += (sender, e) =>
            {
                lock (ComboStats)
                {
                    if (e.NewItems != null)
                    {
                        foreach (Account account in e.NewItems)
                            ComboStats.ByState[account.State]++;
                    }

                    if (e.OldItems != null)
                    {
                        foreach (Account account in e.OldItems)
                        {
                            ComboStats.ByState[account.State]--;
                            if (account.State == AccountState.Reserved)
                                account.MasterToken.Cancel();
                        }
                    }

                    if (Accounts.Count == 0)
                    {
                        StopCommand.Execute(null);
                        ComboStats.Clear();
                    }
                }
            };

            #region Commands

            StartCommand = new LambdaCommand(OnStartCommandExecuted, CanExecuteStartCommand);
            PauseCommand = new LambdaCommand(OnPauseCommandExecuted, CanExecutePauseCommand);
            ContinueCommand = new LambdaCommand(OnContinueCommandExecuted, CanExecuteContinueCommand);
            StopCommand = new LambdaCommand(OnStopCommandExecuted, CanExecuteStopCommand);

            LoadCombosCommand = new LambdaCommand(OnLoadCombosCommandExecuted, CanExecuteLoadCombosCommand);
            ClearCombosCommand = new LambdaCommand(OnClearCombosCommandExecuted, CanExecuteClearCombosCommand);
            StopAndClearCombosCommand = new LambdaCommand(OnStopAndClearCombosCommandExecuted, CanExecuteStopAndClearCombosCommand);
            LoadProxiesCommand = new LambdaCommand(OnLoadProxiesCommandExecuted, CanExecuteLoadProxiesCommand);
            ExportCommand = new LambdaCommand(OnExportCommandExecuted, CanExecuteExportCommand);

            CopyAccountCredentialsCommand = new LambdaCommand(OnCopyAccountCredentialsCommandExecuted, CanExecuteCopyAccountCredentialsCommand);
            RemoveAccountCommand = new LambdaCommand(OnRemoveAccountCommandExecuted, CanExecuteRemoveAccountCommand);

            ContactAuthorCommand = new LambdaCommand(OnContactAuthorCommandExecuted, CanExecuteContactAuthorCommand);

            #endregion

            AppSettings.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(AppSettings.ThreadCount))
                    distributor.ThreadCount = AppSettings.ThreadCount;
            };

            Accounts.CollectionChanged += (sender, e) =>
                IsGreetingVisible = Accounts.Count == 0;

            Task.Run(() =>
            {
                while (true)
                {
                    ProxyDispenser.Refresh();

                    int checkedCount = Accounts.Count
                        - ComboStats.ByState[AccountState.Unchecked]
                        - ComboStats.ByState[AccountState.Reserved];
                    float percentageChecked = checkedCount
                        / Math.Max(Accounts.Count, 1.0f) * 100;
                    Description = $"{percentageChecked:0}%";

                    if (PipelineState != PipelineState.Idle)
                    {
                        TimeSpan elapsed = progressWatch.Elapsed;
                        Description += $" ({elapsed.ToShortDurationString()} затрачено";

                        if (percentageChecked > 0 && percentageChecked < 100)
                        {
                            TimeSpan left = elapsed * (100 - percentageChecked) / percentageChecked;
                            Description += $", {left.ToShortDurationString()} осталось";
                        }
                        Description += ")";
                    }

                    Task.Delay(1000).Wait();
                }
            });
        }
    }
}
