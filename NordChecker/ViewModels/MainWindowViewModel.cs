using HandyControl.Themes;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
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
using System.Linq.Expressions;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using Leaf.xNet;
using System.Windows.Media.Effects;
using HandyControl.Tools.Command;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace NordChecker.ViewModels
{
    public enum PipelineState
    {
        Idle,
        Paused,
        Working
    }

    public class MainWindowViewModel : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        private INavigationService navigationService;

        private bool _IsContentLocked;
        public bool IsContentLocked
        {
            get => _IsContentLocked;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsContentLocked, value, PropertyChanged);
                ContentVisualEffect = value ? new BlurEffect() { Radius = 45 } : null;
            }
        }

        private Effect _ContentVisualEffect;
        public Effect ContentVisualEffect
        {
            get => _ContentVisualEffect;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ContentVisualEffect, value, PropertyChanged);
        }

        public AppSettings AppSettings { get; set; }
        public ExportSettings ExportSettings { get; set; }

        private ThreadMasterToken masterToken;

        private ComboBaseViewModel _ComboBase = new ComboBaseViewModel();
        public ComboBaseViewModel ComboBase
        {
            get => _ComboBase;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ComboBase, value, PropertyChanged);
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
                ComboBase.Refresh();
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

        private ThreadDistributor<Account> distributor;
        private Checker checker = new Checker(7000);

        #region Commands

        #region StartCommand

        public ICommand StartCommand { get; }

        private bool CanExecuteStartCommand(object parameter) => PipelineState == PipelineState.Idle;

        private void OnStartCommandExecuted(object parameter)
        {
            Log.Information("OnStartCommandExecuted");

            PipelineState = PipelineState.Working;
            masterToken = new ThreadMasterToken();
            new Thread(() =>
            {
                distributor = new ThreadDistributor<Account>(
                    AppSettings.ThreadCount,
                    ComboBase.Accounts,
                    (acc) =>
                    {
                        if (acc.State == AccountState.Unchecked)
                        {
                            acc.State = AccountState.Reserved;
                            return true;
                        }
                        return false;
                    },
                    checker.ProcessAccount,
                    masterToken);
            })
            { IsBackground = true }.Start();
        }

        #endregion

        #region PauseCommand

        public ICommand PauseCommand { get; }

        private bool CanExecutePauseCommand(object parameter) => PipelineState == PipelineState.Working;

        private void OnPauseCommandExecuted(object parameter)
        {
            Log.Information("OnStopCommandExecuted");

            PipelineState = PipelineState.Paused;
            masterToken.Pause();
        }

        #endregion

        #region ContinueCommand

        public ICommand ContinueCommand { get; }

        private bool CanExecuteContinueCommand(object parameter) => PipelineState == PipelineState.Paused;

        private void OnContinueCommandExecuted(object parameter)
        {
            Log.Information("OnContinueCommandExecuted");
            PipelineState = PipelineState.Working;
            masterToken.Continue();
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
                                ComboBase.MismatchedCount++;
                                Log.Debug("Line \"{line}\" has been skipped as mismatched", line);
                                continue;
                            }

                            if (AppSettings.AreComboDuplicatesSkipped)
                            {
                                if (ComboBase.Accounts.Any(a => a.Credentials == account.Credentials) ||
                                    cache.Any(a => a.Credentials == account.Credentials))
                                {
                                    ComboBase.DuplicatesCount++;
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

                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        foreach (Account account in cache)
                            ComboBase.Accounts.Add(account);
                        ComboBase.LoadedCount += cache.Count;
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
            //masterToken.Cancel();
            ComboBase.LoadedCount = 0;
            ComboBase.MismatchedCount = 0;
            ComboBase.DuplicatesCount = 0;
            ComboBase.Accounts.Clear();
            PipelineState = PipelineState.Idle;
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
            && ComboBase.Accounts.Count > 0;

        private void OnExportCommandExecuted(object parameter)
        {
            Log.Information("OnExportCommandExecuted");
            navigationService.Navigate(new ExportPage(ExportSettings.Clone() as ExportSettings));
        }

        #endregion

        #region ContactAuthorCommand

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

        #endregion

        #endregion

        public MainWindowViewModel(INavigationService navigationService, AppSettings appSettings, ExportSettings exportSettings)
        {
            this.navigationService = navigationService;
            AppSettings = appSettings;
            ExportSettings = exportSettings;

            #region Commands

            StartCommand = new LambdaCommand(OnStartCommandExecuted, CanExecuteStartCommand);
            PauseCommand = new LambdaCommand(OnPauseCommandExecuted, CanExecutePauseCommand);
            ContinueCommand = new LambdaCommand(OnContinueCommandExecuted, CanExecuteContinueCommand);

            LoadCombosCommand = new LambdaCommand(OnLoadCombosCommandExecuted, CanExecuteLoadCombosCommand);
            ClearCombosCommand = new LambdaCommand(OnClearCombosCommandExecuted, CanExecuteClearCombosCommand);
            LoadProxiesCommand = new LambdaCommand(OnLoadProxiesCommandExecuted, CanExecuteLoadProxiesCommand);
            ExportCommand = new LambdaCommand(OnExportCommandExecuted, CanExecuteExportCommand);

            ContactAuthorCommand = new LambdaCommand(OnContactAuthorCommandExecuted, CanExecuteContactAuthorCommand);

            #endregion

            AppSettings.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(AppSettings.ThreadCount))
                    distributor.ThreadCount = AppSettings.ThreadCount;
            };

            ComboBase.Accounts.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
                IsGreetingVisible = ComboBase.Accounts.Count == 0;

            DispatcherTimer updateTimer = new DispatcherTimer();
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            updateTimer.Tick += (sender, e) =>
            {
                ComboBase.Refresh();
                ProxyDispenser.Refresh();
            };

            updateTimer.Start();
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Task.Delay(5000).Wait();
                (Application.Current.MainWindow as NavigationWindow)
                .Navigate(new MainPage(new MainPageViewModel(navigationService), AppSettings));
            }).Wait();
            //navigationService.Navigate(typeof(MainPage));
        }
    }
}
