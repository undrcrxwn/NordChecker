using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
using NordChecker.Commands;
using NordChecker.Models;
using NordChecker.Shared;
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

        public AppSettings Settings { get; set; } = new AppSettings();
        public PipelineState PipelineState { get; set; } = PipelineState.Idle;

        private string _Title = "NordVPN Checker";
        public string Title
        {
            get => _Title;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Title, value, PropertyChanged);
        }

        #region Stats

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

        #region Arc Progress

        private Visibility _ArcPremiumVisibility = Visibility.Visible;
        public Visibility ArcPremiumVisibility
        {
            get => _ArcPremiumVisibility;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcPremiumVisibility, value, PropertyChanged);
        }

        private float _ArcStartAnglePremium = 0;
        public float ArcStartAnglePremium
        {
            get => _ArcStartAnglePremium;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcStartAnglePremium, value, PropertyChanged);
        }

        private float _ArcEndAnglePremium = 1;
        public float ArcEndAnglePremium
        {
            get => _ArcEndAnglePremium;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcEndAnglePremium, value, PropertyChanged);
        }



        private Visibility _ArcFreeVisibility = Visibility.Visible;
        public Visibility ArcFreeVisibility
        {
            get => _ArcFreeVisibility;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcFreeVisibility, value, PropertyChanged);
        }

        private float _ArcStartAngleFree = 0;
        public float ArcStartAngleFree
        {
            get => _ArcStartAngleFree;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcStartAngleFree, value, PropertyChanged);
        }

        private float _ArcEndAngleFree = 1;
        public float ArcEndAngleFree
        {
            get => _ArcEndAngleFree;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcEndAngleFree, value, PropertyChanged);
        }



        private Visibility _ArcInvalidVisibility = Visibility.Visible;
        public Visibility ArcInvalidVisibility
        {
            get => _ArcInvalidVisibility;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcInvalidVisibility, value, PropertyChanged);
        }

        private float _ArcStartAngleInvalid = 183;
        public float ArcStartAngleInvalid
        {
            get => _ArcStartAngleInvalid;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcStartAngleInvalid, value, PropertyChanged);
        }

        private float _ArcEndAngleInvalid = 267;
        public float ArcEndAngleInvalid
        {
            get => _ArcEndAngleInvalid;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcEndAngleInvalid, value, PropertyChanged);
        }



        private Visibility _ArcUncheckedVisibility = Visibility.Visible;
        public Visibility ArcUncheckedVisibility
        {
            get => _ArcUncheckedVisibility;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcUncheckedVisibility, value, PropertyChanged);
        }

        private float _ArcStartAngleUnchecked = 273;
        public float ArcStartAngleUnchecked
        {
            get => _ArcStartAngleUnchecked;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcStartAngleUnchecked, value, PropertyChanged);
        }

        private float _ArcEndAngleUnchecked = 357;
        public float ArcEndAngleUnchecked
        {
            get => _ArcEndAngleUnchecked;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcEndAngleUnchecked, value, PropertyChanged);
        }

        #endregion

        #endregion

        //public ComboBase CurrentBase { get; set; } = new ComboBase();
        private ComboBase _CurrentBase = new ComboBase();
        public ComboBase CurrentBase
        {
            get => _CurrentBase;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _CurrentBase, value, PropertyChanged);
        }

        #region Displayed Items

        private bool _AreUncheckedDisplayed = true;
        public bool AreUncheckedDisplayed
        {
            get => _AreUncheckedDisplayed;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreUncheckedDisplayed, value, PropertyChanged);
        }

        private bool _AreInvalidDisplayed = true;
        public bool AreInvalidDisplayed
        {
            get => _AreInvalidDisplayed;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreInvalidDisplayed, value, PropertyChanged);
        }

        private bool _AreFreeDisplayed = true;
        public bool AreFreeDisplayed
        {
            get => _AreFreeDisplayed;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreFreeDisplayed, value, PropertyChanged);
        }

        private bool _ArePremiumDisplayed = true;
        public bool ArePremiumDisplayed
        {
            get => _ArePremiumDisplayed;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArePremiumDisplayed, value, PropertyChanged);
        }

        #endregion

        #endregion

        Checker checker = new Checker();
        ThreadDistributor distributor;

        #region Commands

        #region ContactAuthorCommand

        public ICommand ContactAuthorCommand { get; }

        private bool CanExecuteContactAuthorCommand(object parameter) => true;

        private void OnContactAuthorCommandExecuted(object parameter)
        {
            Process.Start("https://t.me/undrcrxwn");
        }

        #endregion

        #region LoadBaseCommand

        public ICommand LoadBaseCommand { get; }

        private bool CanExecuteLoadBaseCommand(object parameter) => true;

        private void OnLoadBaseCommandExecuted(object parameter)
        {
            HandyControl.Controls.MessageBox.Show("OnLoadBaseCommandExecuted");
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
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Account? account = Parser.Parse(line);
                        if (account == null)
                        {
                            MismatchedCount++;
                            continue;
                        }

                        if (Settings.AreComboDuplicatesSkipped &&
                            CurrentBase.Accounts.Any(a => a.Credentials == account.Credentials))
                        {
                            DuplicatesCount++;
                            continue;
                        }

                        Application.Current.Dispatcher.Invoke(() => CurrentBase.Accounts.Add(account));
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }

        #endregion

        #region StartCommand

        public ICommand StartCommand { get; }

        private bool CanExecuteStartCommand(object parameter) => PipelineState != PipelineState.Working;

        private void OnStartCommandExecuted(object parameter)
        {
            HandyControl.Controls.MessageBox.Show("OnStartCommandExecuted");
            PipelineState = PipelineState.Working;
            new Thread(() =>
            {
                while (PipelineState == PipelineState.Working)
                {
                    List<Account> chunk = new List<Account>();
                    lock (CurrentBase.Accounts)
                    {
                        chunk = CurrentBase.Accounts
                            .Where(a => a.State == AccountState.Unchecked)
                            .ToList().Take(5 * Settings.ThreadCount)
                            .ToList();
                    }

                    foreach (Account account in chunk)
                    {
                        account.State = AccountState.Reserved;
                        CancelableAction action = new CancelableAction(checker.ProcessAccount, account);
                        action.OnCanceled += () => account.State = AccountState.Invalid;
                        distributor.Push(action);
                    }

                    /*
                     for (int i = 0; i < 5 * Settings.ThreadCount; i++)
                    {
                        Account account = CurrentBase.Accounts.Find(a => a.State == AccountState.Unchecked);
                        if (account == null)
                            return;
                        account.State = AccountState.Reserved;
                        CancelableAction action = new CancelableAction(checker.ProcessAccount, account);
                        action.OnCanceled += () => account.State = AccountState.Invalid;
                        distributor.Push(action);
                    }

                    while (distributor.Threads.All(t => t.Count > 0))
                        Task.Delay(1000).Wait();
                    Task.Delay(25).Wait();
                     */

                    while (distributor.Threads.All(t => t.Count > 0))
                        Task.Delay(1000).Wait();
                    Task.Delay(25).Wait();
                }
            }).Start();
        }

        #endregion

        #region StopCommand

        public ICommand StopCommand { get; }

        private bool CanExecuteStopCommand(object parameter) => PipelineState == PipelineState.Working;

        private void OnStopCommandExecuted(object parameter)
        {
            HandyControl.Controls.MessageBox.Show("OnStopCommandExecuted");
            PipelineState = PipelineState.Paused;
        }

        #endregion

        #endregion

        #region UI

        private Dictionary<AccountState, Arc> _Arcs = new Dictionary<AccountState, Arc>()
        {
            { AccountState.Premium, new Arc(0, 1, Visibility.Hidden) },
            { AccountState.Free, new Arc(0, 1, Visibility.Hidden) },
            { AccountState.Invalid, new Arc(0, 1, Visibility.Hidden) },
            { AccountState.Reserved, new Arc(0, 1, Visibility.Hidden) },
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

        private void UpdateStats(object sender, EventArgs e)
        {
            #region Arc Progress
            Stats = CurrentBase.CalculateStats();
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
            distributor = new ThreadDistributor(Settings.ThreadCount, 600000);

            DispatcherTimer updateTimer = new DispatcherTimer();
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            updateTimer.Tick += new EventHandler(UpdateStats);
            updateTimer.Tick += (object sender, EventArgs e) =>
            {
                Trace.TraceInformation(CurrentBase.Accounts.Count.ToString());
            };
            //updateTimer.Tick += (object sender, EventArgs e) =>
            //    CurrentBase.State = distributor.CountActiveThreads() > 0
            //    ? ComboBaseState.Processing : ComboBaseState.Idle;
            updateTimer.Start();

            #region Commands

            StartCommand = new LambdaCommand(OnStartCommandExecuted, CanExecuteStartCommand);
            StopCommand = new LambdaCommand(OnStopCommandExecuted, CanExecuteStopCommand);
            LoadBaseCommand = new LambdaCommand(OnLoadBaseCommandExecuted, CanExecuteLoadBaseCommand);
            ContactAuthorCommand = new LambdaCommand(OnContactAuthorCommandExecuted, CanExecuteContactAuthorCommand);

            #endregion
        }
    }
}
