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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using static NordChecker.ViewModels.MainWindowViewModel;

namespace NordChecker.ViewModels
{
    #region Converters

    [ValueConversion(typeof(int), typeof(string))]
    public class NumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nfi = (NumberFormatInfo)culture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            return ((int)value).ToString("#,0", nfi);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result;
            if (int.TryParse(value.ToString(), NumberStyles.Any, culture, out result))
                return result;
            else if (int.TryParse(value.ToString(), NumberStyles.Any, culture, out result))
                return result;
            return value;
        }
    }

    [ValueConversion(typeof(AccountState), typeof(string))]
    public class AccState2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (AccountState)value switch
            {
                AccountState.Unchecked => "🕒 В очереди",
                AccountState.Reserved => "🕖 В обработке",
                AccountState.Invalid => "❌ Невалидный",
                AccountState.Free => "✔️ Бесплатный",
                AccountState.Premium => "⭐ Премиум",
                _ => throw new ArgumentException()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string lowerCase = value.ToString().ToLower();
            if (lowerCase.Contains("в очереди")) return AccountState.Unchecked;
            if (lowerCase.Contains("в обработке")) return AccountState.Reserved;
            if (lowerCase.Contains("невалидный")) return AccountState.Invalid;
            if (lowerCase.Contains("бесплатный")) return AccountState.Free;
            if (lowerCase.Contains("премиум")) return AccountState.Premium;
            throw new ArgumentException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class Boolean2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture) => (bool)value ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture) => (Visibility)value == Visibility.Visible;
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture) => !(bool)value;

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture) => !(bool)value;
    }

    #endregion

    public enum PipelineState
    {
        Idle,
        Paused,
        Working
    }

    public class Arc : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private float _StartAngle;
        public float StartAngle
        {
            get => _StartAngle;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _StartAngle, value, PropertyChanged);
        }

        private float _EndAngle;
        public float EndAngle
        {
            get => _EndAngle;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _EndAngle, value, PropertyChanged);
        }


        private Visibility _Visibility;

        public Visibility Visibility
        {
            get => _Visibility;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Visibility, value, PropertyChanged);
        }

        public Arc(float startAngle, float endAngle, Visibility visibility)
        {
            StartAngle = startAngle;
            EndAngle = endAngle;
            Visibility = visibility;
        }
    }

    internal class MainWindowViewModel : INotifyPropertyChangedAdvanced
    {
        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        public AppSettings Settings => (Application.Current as App).Settings;

        private ThreadMasterToken masterToken = new ThreadMasterToken();

        public bool IsPipelineIdle { get => PipelineState == PipelineState.Idle; }
        public bool IsPipelinePaused { get => PipelineState == PipelineState.Paused; }
        public bool IsPipelineWorking { get => PipelineState == PipelineState.Working; }

        private PipelineState _PipelineState = PipelineState.Idle;
        public PipelineState PipelineState
        {
            get => _PipelineState;
            set
            {
                INotifyPropertyChangedAdvanced inst = this;
                inst.Set(ref _PipelineState, value, PropertyChanged, LogEventLevel.Information);
                inst.OnPropertyChanged(PropertyChanged, Utils.GetMemberName(() => IsPipelineIdle));
                inst.OnPropertyChanged(PropertyChanged, Utils.GetMemberName(() => IsPipelinePaused));
                inst.OnPropertyChanged(PropertyChanged, Utils.GetMemberName(() => IsPipelineWorking));
                UpdateStats();
            }
        }

        private string _Title = "NordVPN Checker";
        public string Title
        {
            get => _Title;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Title, value, PropertyChanged);
        }

        public string _GreetingTitle;
        public string GreetingTitle
        {
            get => _GreetingTitle;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _GreetingTitle, value, PropertyChanged);
            }
        }

        private bool _IsGreetingVisible = true;
        public bool IsGreetingVisible
        {
            get => _IsGreetingVisible;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsGreetingVisible, value, PropertyChanged);
        }

        #region Stats

        private int _LoadedCount;
        public int LoadedCount
        {
            get => _LoadedCount;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _LoadedCount, value, PropertyChanged);
        }

        private int _MismatchedCount;
        public int MismatchedCount
        {
            get => _MismatchedCount;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _MismatchedCount, value, PropertyChanged);
        }

        private int _DuplicatesCount;
        public int DuplicatesCount
        {
            get => _DuplicatesCount;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _DuplicatesCount, value, PropertyChanged);
        }

        #endregion

        private ComboBase _ComboBase = new ComboBase();
        public ComboBase ComboBase
        {
            get => _ComboBase;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ComboBase, value, PropertyChanged);
        }

        #endregion

        private ThreadDistributor<Account> distributor;
        Checker checker = new Checker(7 * 1000);

        #region Commands

        #region StartCommand

        public ICommand StartCommand { get; }

        private bool CanExecuteStartCommand(object parameter) => PipelineState != PipelineState.Working;

        private void OnStartCommandExecuted(object parameter)
        {
            Log.Information("OnStartCommandExecuted");

            PipelineState = PipelineState.Working;
            masterToken = new ThreadMasterToken();
            new Thread(() =>
            {
                distributor = new ThreadDistributor<Account>(
                    Settings.ThreadCount,
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
                dialog.Filter = "NordVPN Base|*.txt|Все файлы|*.*";

                bool? result = dialog.ShowDialog();
                if (result != true)
                    return;

                StreamReader reader = new StreamReader(File.OpenRead(dialog.FileName));
                Task.Factory.StartNew(() =>
                {
                    Log.Information("Reading combos from {file}", dialog.FileName);
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    string line;
                    List<Account> cache = new List<Account>();
                    while ((line = reader.ReadLine()) != null)
                    {
                        Account account;
                        try
                        {
                            account = Parser.Parse(line);
                        }
                        catch
                        {
                            MismatchedCount++;
                            Log.Debug("Line \"{line}\" has been skipped as mismatched", line);
                            continue;
                        }

                        if (Settings.AreComboDuplicatesSkipped)
                        {
                            if (ComboBase.Accounts.Any(a => a.Credentials == account.Credentials) ||
                                cache.Any(a => a.Credentials == account.Credentials))
                            {
                                DuplicatesCount++;
                                Log.Debug("Account {credentials} has been skipped as duplicate", account.Credentials);
                                continue;
                            }
                        }

                        cache.Add(account);
                        Log.Debug("Account {credentials} has been added to the cache", account.Credentials);
                    }

                    watch.Stop();
                    Log.Information("{total} accounts have been extracted from {file} in {elapsed}ms",
                        cache.Count, dialog.FileName, watch.ElapsedMilliseconds);

                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        foreach (Account account in cache)
                            ComboBase.Accounts.Add(account);
                        LoadedCount += cache.Count;
                    });
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }

        #endregion

        #region ClearComboCommand

        public ICommand ClearComboCommand { get; }

        private bool CanExecuteClearComboCommand(object parameter) => PipelineState != PipelineState.Working;

        private void OnClearComboCommandExecuted(object parameter)
        {
            Log.Information("OnClearComboCommandExecuted");
            LoadedCount = 0;
            MismatchedCount = 0;
            DuplicatesCount = 0;
            ComboBase.Accounts.Clear();
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

        #region UI

        private Dictionary<AccountState, bool> _VisibilityFilters = new Dictionary<AccountState, bool>()
        {
            { AccountState.Premium,   true },
            { AccountState.Free,      true },
            { AccountState.Invalid,   true },
            { AccountState.Reserved,  true },
            { AccountState.Unchecked, true }
        };

        public Dictionary<AccountState, bool> VisibilityFilters
        {
            get => _VisibilityFilters;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _VisibilityFilters, value, PropertyChanged);
        }

        private Dictionary<AccountState, Arc> _Arcs = new Dictionary<AccountState, Arc>()
        {
            { AccountState.Premium,   new Arc(0, 1, Visibility.Hidden) },
            { AccountState.Free,      new Arc(0, 1, Visibility.Hidden) },
            { AccountState.Invalid,   new Arc(0, 1, Visibility.Hidden) },
            { AccountState.Reserved,  new Arc(0, 1, Visibility.Hidden) },
            { AccountState.Unchecked, new Arc(0, 1, Visibility.Hidden) }
        };

        public Dictionary<AccountState, Arc> Arcs
        {
            get => _Arcs;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Arcs, value, PropertyChanged);
        }

        private Dictionary<AccountState, int> _Stats = new Dictionary<AccountState, int>();
        public Dictionary<AccountState, int> Stats
        {
            get => _Stats;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Stats, value, PropertyChanged);
        }

        private void UpdateStats()
        {
            #region Arc Progress

            Stats = ComboBase.CalculateStats();
            int loaded = Math.Max(1, Stats.Values.Sum());
            Dictionary<AccountState, float> shares =
                Stats.ToDictionary(p => p.Key, p => (float)p.Value / loaded);

            float margin = 6;
            float pivot = margin / 2;
            float maxPossibleAngle = 360 - (shares.Values.Count(v => v > 0) * margin);
            foreach (var (state, share) in shares)
            {
                if (share == 0)
                {
                    Arcs[state].StartAngle = 0;
                    Arcs[state].EndAngle = 0;
                    Arcs[state].Visibility = Visibility.Hidden;
                }
                else if (share == 1)
                {
                    Arcs[state].StartAngle = 0;
                    Arcs[state].EndAngle = 360;
                    Arcs[state].Visibility = Visibility.Visible;
                }
                else
                {
                    Arcs[state].StartAngle = pivot;
                    pivot += share * maxPossibleAngle;
                    Arcs[state].EndAngle = pivot;
                    pivot += margin;
                    Arcs[state].Visibility = Visibility.Visible;
                }
            }

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            Application.Current.MainWindow.TaskbarItemInfo ??= new System.Windows.Shell.TaskbarItemInfo();

            if (Stats[AccountState.Unchecked] + Stats[AccountState.Reserved] > 0)
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

            #endregion
        }

        #endregion

        public MainWindowViewModel()
        {
            #region Commands

            StartCommand = new LambdaCommand(OnStartCommandExecuted, CanExecuteStartCommand);
            PauseCommand = new LambdaCommand(OnPauseCommandExecuted, CanExecutePauseCommand);
            ContinueCommand = new LambdaCommand(OnContinueCommandExecuted, CanExecuteContinueCommand);

            LoadCombosCommand = new LambdaCommand(OnLoadCombosCommandExecuted, CanExecuteLoadCombosCommand);
            ClearComboCommand = new LambdaCommand(OnClearComboCommandExecuted, CanExecuteClearComboCommand);

            ContactAuthorCommand = new LambdaCommand(OnContactAuthorCommandExecuted, CanExecuteContactAuthorCommand);

            #endregion

            GreetingTitle = Settings.IsDeveloperModeEnabled ? "👨🏻‍🔬" : "🦄";
            Settings.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == Utils.GetMemberName(() => Settings.IsDeveloperModeEnabled))
                    GreetingTitle = Settings.IsDeveloperModeEnabled ? "👨🏻‍🔬" : "🦄";
                else if (e.PropertyName == Utils.GetMemberName(() => Settings.ThreadCount))
                    distributor.ThreadCount = Settings.ThreadCount;
            };

            ComboBase.Accounts.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                IsGreetingVisible = ComboBase.Accounts.Count == 0;
            };

            DispatcherTimer updateTimer = new DispatcherTimer();
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            updateTimer.Tick += (object sender, EventArgs e) => UpdateStats();
            //updateTimer.Tick += (object sender, EventArgs e) =>
            //    ComboBase.State = distributor.CountActiveThreads() > 0
            //    ? ComboBaseState.Processing : ComboBaseState.Idle;
            updateTimer.Start();
        }
    }
}
