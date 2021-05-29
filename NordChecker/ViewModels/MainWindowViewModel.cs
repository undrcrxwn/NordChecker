using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
using NordChecker.Commands;
using NordChecker.Models;
using NordChecker.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace NordChecker.ViewModels
{
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
                AccountState.Unchecked => "🕑 В очереди",
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
            if (lowerCase.Contains("невалидный")) return AccountState.Invalid;
            if (lowerCase.Contains("бесплатный")) return AccountState.Free;
            if (lowerCase.Contains("премиум")) return AccountState.Premium;
            throw new ArgumentException();
        }
    }

    internal class MainWindowViewModel : ViewModel
    {
        #region Properties

        private string _Title = "NordVPN Checker";
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, "NordVPN Checker" + (string.IsNullOrEmpty(value) ? "" : $" I {value}"));
        }

        #region Base Stats

        private int _BaseLoaded;
        public int BaseLoaded
        {
            get => _BaseLoaded;
            set => Set(ref _BaseLoaded, value);
        }

        private int _BasePremium;
        public int BasePremium
        {
            get => _BasePremium;
            set => Set(ref _BasePremium, value);
        }

        private int _BaseFree;
        public int BaseFree
        {
            get => _BaseFree;
            set => Set(ref _BaseFree, value);
        }

        private int _BaseInvalid;
        public int BaseInvalid
        {
            get => _BaseInvalid;
            set => Set(ref _BaseInvalid, value);
        }

        private int _BaseUnchecked;
        public int BaseUnchecked
        {
            get => _BaseUnchecked;
            set => Set(ref _BaseUnchecked, value);
        }

        private int _BaseMismatched;
        private object _BaseMismatchedLocker = new object();
        public int BaseMismatched
        {
            get => _BaseMismatched;
            set => Set(ref _BaseMismatched, value);
        }

        #region Arc Progress

        private Visibility _ArcPremiumVisibility = Visibility.Visible;
        public Visibility ArcPremiumVisibility
        {
            get => _ArcPremiumVisibility;
            set => Set(ref _ArcPremiumVisibility, value);
        }

        private float _ArcStartAnglePremium = 0;
        public float ArcStartAnglePremium
        {
            get => _ArcStartAnglePremium;
            set => Set(ref _ArcStartAnglePremium, value);
        }

        private float _ArcEndAnglePremium = 1;
        public float ArcEndAnglePremium
        {
            get => _ArcEndAnglePremium;
            set => Set(ref _ArcEndAnglePremium, value);
        }



        private Visibility _ArcFreeVisibility = Visibility.Visible;
        public Visibility ArcFreeVisibility
        {
            get => _ArcFreeVisibility;
            set => Set(ref _ArcFreeVisibility, value);
        }

        private float _ArcStartAngleFree = 0;
        public float ArcStartAngleFree
        {
            get => _ArcStartAngleFree;
            set => Set(ref _ArcStartAngleFree, value);
        }

        private float _ArcEndAngleFree = 1;
        public float ArcEndAngleFree
        {
            get => _ArcEndAngleFree;
            set => Set(ref _ArcEndAngleFree, value);
        }



        private Visibility _ArcInvalidVisibility = Visibility.Visible;
        public Visibility ArcInvalidVisibility
        {
            get => _ArcInvalidVisibility;
            set => Set(ref _ArcInvalidVisibility, value);
        }

        private float _ArcStartAngleInvalid = 183;
        public float ArcStartAngleInvalid
        {
            get => _ArcStartAngleInvalid;
            set => Set(ref _ArcStartAngleInvalid, value);
        }

        private float _ArcEndAngleInvalid = 267;
        public float ArcEndAngleInvalid
        {
            get => _ArcEndAngleInvalid;
            set => Set(ref _ArcEndAngleInvalid, value);
        }



        private Visibility _ArcUncheckedVisibility = Visibility.Visible;
        public Visibility ArcUncheckedVisibility
        {
            get => _ArcUncheckedVisibility;
            set => Set(ref _ArcUncheckedVisibility, value);
        }

        private float _ArcStartAngleUnchecked = 273;
        public float ArcStartAngleUnchecked
        {
            get => _ArcStartAngleUnchecked;
            set => Set(ref _ArcStartAngleUnchecked, value);
        }

        private float _ArcEndAngleUnchecked = 357;
        public float ArcEndAngleUnchecked
        {
            get => _ArcEndAngleUnchecked;
            set => Set(ref _ArcEndAngleUnchecked, value);
        }

        #endregion

        #endregion

        public class Base
        {
            public ObservableCollection<Account> Accounts { get; set; } = new ObservableCollection<Account>();

            private bool _IsReady = true;
            public bool IsReady
            {
                get => _IsReady;
                set
                {
                    _IsReady = value;
                    Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
                }
            }

            public void Lock() => IsReady = false;
            public void Unlock() => IsReady = true;
        }

        private Base _CurrentBase = new Base();
        public Base CurrentBase
        {
            get => _CurrentBase;
            set => Set(ref _CurrentBase, value);
        }

        #region Displayed Items

        private bool _AreUncheckedDisplayed = true;
        public bool AreUncheckedDisplayed
        {
            get => _AreUncheckedDisplayed;
            set => Set(ref _AreUncheckedDisplayed, value);
        }

        private bool _AreInvalidDisplayed = true;
        public bool AreInvalidDisplayed
        {
            get => _AreInvalidDisplayed;
            set => Set(ref _AreInvalidDisplayed, value);
        }

        private bool _AreFreeDisplayed = true;
        public bool AreFreeDisplayed
        {
            get => _AreFreeDisplayed;
            set => Set(ref _AreFreeDisplayed, value);
        }

        private bool _ArePremiumDisplayed = true;
        public bool ArePremiumDisplayed
        {
            get => _ArePremiumDisplayed;
            set => Set(ref _ArePremiumDisplayed, value);
        }

        #endregion

        #endregion

        Checker checker = new Checker();
        ThreadDistributor distributor = new ThreadDistributor(500, 600000);

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

        private bool CanExecuteLoadBaseCommand(object parameter) => CurrentBase.IsReady;

        private void ProcessLine(string line)
        {
            Account? account = Parser.Parse(line);
            if (account == null)
            {
                lock (_BaseMismatchedLocker)
                    BaseMismatched++;
                return;
            }

            Application.Current.Dispatcher.Invoke(() => CurrentBase.Accounts.Add(account));

            CancelableAction action = new CancelableAction(checker.ProcessAccount, account);
            action.OnCanceled += () => { account.State = AccountState.Invalid; };
            distributor.Push(action);
        }

        private void OnLoadBaseCommandExecuted(object parameter)
        {
            HandyControl.Controls.MessageBox.Show("OnLoadBaseCommandExecuted");
            Task.Run(() =>
            {
                var dlg = new OpenFileDialog();
                dlg.DefaultExt = ".txt";
                dlg.Filter = "NordVPN Base|*.txt|Все файлы|*.*";

                bool? result = dlg.ShowDialog();
                if (result != true)
                    return;

                CurrentBase.Lock();

                StreamReader reader = new StreamReader(File.OpenRead(dlg.FileName));

                Task.Run(() =>
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string copy = line;
                        //Thread thread = new Thread(() => { ProcessLine(copy); });
                        //thread.Start();

                        ProcessLine(copy);
                        //Thread thread = new Thread(() => ProcessLine(copy, tasks));
                        //thread.Join();
                    }
                });

                //Task.WaitAll(tasks.ToArray());
                CurrentBase.Unlock();
            });
        }

        #endregion

        #endregion

        #region UI

        private void UpdateStats(object sender, EventArgs e)
        {
            Trace.WriteLine($"[LOG] MAX={distributor.MaxThreadAmount} ACTIVE={QueueThread.ACTIVE}");

            if (CurrentBase == null ||
                CurrentBase.Accounts == null)
                return;

            BaseLoaded = CurrentBase.Accounts.Count;
            BasePremium = CurrentBase.Accounts.Count(a => a.State == AccountState.Premium);
            BaseFree = CurrentBase.Accounts.Count(a => a.State == AccountState.Free);
            BaseUnchecked = CurrentBase.Accounts.Count(a => a.State == AccountState.Unchecked);
            BaseInvalid = CurrentBase.Accounts.Count(a => a.State == AccountState.Invalid);

            #region Arc Progress

            int loaded = Math.Max(1, BaseLoaded);
            float progressPremium = (float)BasePremium / loaded;
            float progressFree = (float)BaseFree / loaded;
            float progressInvalid = (float)BaseInvalid / loaded;
            float progressUnchecked = (float)BaseUnchecked / loaded;

            int visibleArcsAmount = new List<float>() {
                progressPremium,
                progressFree,
                progressInvalid,
                progressUnchecked
            }.Count(x => x > 0);

            if (visibleArcsAmount == 0)
            {
                ArcPremiumVisibility = Visibility.Hidden;
                ArcFreeVisibility = Visibility.Hidden;
                ArcInvalidVisibility = Visibility.Hidden;
                ArcUncheckedVisibility = Visibility.Hidden;
                return;
            }

            float spacing = 6;
            float maxPossibleShare = 360 - (spacing * visibleArcsAmount);
            if (visibleArcsAmount == 1)
                maxPossibleShare = 360;

            float pivot = spacing / -2;

            if (progressPremium > 0)
            {
                ArcStartAnglePremium = (pivot += spacing);
                ArcEndAnglePremium = (pivot += (maxPossibleShare * progressPremium));
                ArcPremiumVisibility = Visibility.Visible;
            }
            else
                ArcPremiumVisibility = Visibility.Hidden;

            if (progressFree > 0)
            {
                ArcStartAngleFree = (pivot += spacing);
                ArcEndAngleFree = (pivot += (maxPossibleShare * progressFree));
                ArcFreeVisibility = Visibility.Visible;
            }
            else
                ArcFreeVisibility = Visibility.Hidden;

            if (progressInvalid > 0)
            {
                ArcStartAngleInvalid = (pivot += spacing);
                ArcEndAngleInvalid = (pivot += (maxPossibleShare * progressInvalid));
                ArcInvalidVisibility = Visibility.Visible;
            }
            else
                ArcInvalidVisibility = Visibility.Hidden;

            if (progressUnchecked > 0)
            {
                ArcStartAngleUnchecked = (pivot += spacing);
                ArcEndAngleUnchecked = (pivot += (maxPossibleShare * progressUnchecked));
                ArcUncheckedVisibility = Visibility.Visible;
            }
            else
                ArcUncheckedVisibility = Visibility.Hidden;

            Application.Current.MainWindow.TaskbarItemInfo ??= new System.Windows.Shell.TaskbarItemInfo();

            if (progressUnchecked > 0)
            {
                Application.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                Application.Current.MainWindow.TaskbarItemInfo.ProgressValue = 1 - progressUnchecked;
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
            DispatcherTimer uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            uiUpdateTimer.Tick += new EventHandler(UpdateStats);
            uiUpdateTimer.Start();

            /*Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();

                uint counter = 0;
                while (true)
                {
                    Thread th = new Thread(() =>
                    {
                        Thread.Sleep(5000);
                        counter++;
                    });
                    th.Start();

                    Title = counter.ToString();
                }
            });*/

            #region Commands

            LoadBaseCommand = new LambdaCommand(OnLoadBaseCommandExecuted, CanExecuteLoadBaseCommand);
            ContactAuthorCommand = new LambdaCommand(OnContactAuthorCommandExecuted, CanExecuteContactAuthorCommand);

            #endregion
        }
    }
}
