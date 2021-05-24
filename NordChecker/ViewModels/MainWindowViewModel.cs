using Microsoft.Win32;
using NordChecker.Commands;
using NordChecker.Models;
using NordChecker.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        private int _BaseValid;
        public int BaseValid
        {
            get => _BaseValid;
            set => Set(ref _BaseValid, value);
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

        private Visibility _ArcValidVisibility = Visibility.Hidden;
        public Visibility ArcValidVisibility
        {
            get => _ArcValidVisibility;
            set => Set(ref _ArcValidVisibility, value);
        }

        private float _ArcStartAngleValid = 3;
        public float ArcStartAngleValid
        {
            get => _ArcStartAngleValid;
            set => Set(ref _ArcStartAngleValid, value);
        }

        private float _ArcEndAngleValid = 117;
        public float ArcEndAngleValid
        {
            get => _ArcEndAngleValid;
            set => Set(ref _ArcEndAngleValid, value);
        }



        private Visibility _ArcInvalidVisibility = Visibility.Hidden;
        public Visibility ArcInvalidVisibility
        {
            get => _ArcInvalidVisibility;
            set => Set(ref _ArcInvalidVisibility, value);
        }

        private float _ArcStartAngleInvalid = 123;
        public float ArcStartAngleInvalid
        {
            get => _ArcStartAngleInvalid;
            set => Set(ref _ArcStartAngleInvalid, value);
        }

        private float _ArcEndAngleInvalid = 237;
        public float ArcEndAngleInvalid
        {
            get => _ArcEndAngleInvalid;
            set => Set(ref _ArcEndAngleInvalid, value);
        }



        private Visibility _ArcUncheckedVisibility = Visibility.Hidden;
        public Visibility ArcUncheckedVisibility
        {
            get => _ArcUncheckedVisibility;
            set => Set(ref _ArcUncheckedVisibility, value);
        }

        private float _ArcStartAngleUnchecked = 243;
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

                List<Task> tasks = new List<Task>();
                StreamReader reader = new StreamReader(File.OpenRead(dlg.FileName));

                Task.Run(() =>
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string copy = line;
                        Thread thread = new Thread(() => { ProcessLine(copy); });
                        thread.Start();

                        //Prall Task.Run(() => ProcessLine(copy, tasks));
                        //Thread thread = new Thread(() => ProcessLine(copy, tasks));
                        //thread.Join();
                    }
                });

                while (!tasks.All(t => t.IsCompleted))
                    Thread.Sleep(1000);
                //Task.WaitAll(tasks.ToArray());
                CurrentBase.Unlock();
            });
        }

        #endregion

        #endregion

        private void UpdateStats(object sender, EventArgs e)
        {
            if (CurrentBase == null ||
                CurrentBase.Accounts == null)
                return;

            BaseLoaded = CurrentBase.Accounts.Count;
            BaseInvalid = CurrentBase.Accounts.Count(a => a.State == AccountState.Invalid);
            BaseUnchecked = CurrentBase.Accounts.Count(a => a.State == AccountState.Unchecked);
            BaseValid = BaseLoaded - BaseUnchecked - BaseInvalid;

            #region Arc Progress

            int loaded = Math.Max(1, BaseLoaded);
            float progressValid = ((float)BaseValid / loaded);
            float progressInvalid = ((float)BaseInvalid / loaded);
            float progressUnchecked = ((float)BaseUnchecked / loaded);
            float angleValid = progressValid * 348;
            float angleInvalid = progressInvalid * 348;

            ArcStartAngleValid = 3;
            ArcEndAngleValid = ArcStartAngleValid + angleValid;
            ArcValidVisibility = progressValid == 0
                ? ArcValidVisibility = Visibility.Hidden
                : Visibility.Visible;

            ArcStartAngleInvalid = ArcEndAngleValid + 6;
            ArcEndAngleInvalid = ArcStartAngleInvalid + angleInvalid;
            ArcInvalidVisibility = progressInvalid == 0
                ? Visibility.Hidden
                : Visibility.Visible;
            if (progressValid == 0)
                ArcStartAngleInvalid -= 6;

            ArcStartAngleUnchecked = ArcEndAngleInvalid + 6;
            ArcEndAngleUnchecked = 357;
            ArcUncheckedVisibility = progressUnchecked == 0
                ? Visibility.Hidden
                : Visibility.Visible;
            if (ArcStartAngleUnchecked >= ArcEndAngleUnchecked)
                ArcUncheckedVisibility = Visibility.Hidden;
            if (progressValid == 0 && progressInvalid == 0)
                ArcStartAngleUnchecked -= 12;
            #endregion
        }

        public MainWindowViewModel()
        {
            DispatcherTimer uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
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
