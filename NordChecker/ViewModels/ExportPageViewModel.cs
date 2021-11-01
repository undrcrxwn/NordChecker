using Leaf.xNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NordChecker.Commands;
using NordChecker.Models;
using NordChecker.Shared;
using NordChecker.Views;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NordChecker.ViewModels
{
    public class ExportPageViewModel : IPageViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private NavigationService navigationService;

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

        private ObservableCollection<Account> _Accounts;
        public ObservableCollection<Account> Accounts
        {
            get => _Accounts;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Accounts, value, PropertyChanged);
        }

        private ExportSettings _ExportSettings;
        public ExportSettings ExportSettings
        {
            get => _ExportSettings;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ExportSettings, value, PropertyChanged);
        }

        private string _OutputDirectoryPath;
        public string OutputDirectoryPath
        {
            get => _OutputDirectoryPath;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _OutputDirectoryPath, value, PropertyChanged);
                UpdateSettingsRootPath();
            }
        }

        public AccountFormatter Formatter { get; set; }

        private string _OutputPreview;
        public string OutputPreview
        {
            get => _OutputPreview;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _OutputPreview, value, PropertyChanged);
        }

        private bool _CanProceed = false;
        public bool CanProceed
        {
            get => _CanProceed;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _CanProceed, value, PropertyChanged);
        }

        private bool _IsOperationConfirmed = false;
        public bool IsOperationConfirmed
        {
            get => _IsOperationConfirmed;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsOperationConfirmed, value, PropertyChanged);
        }

        #endregion

        #region ChoosePathCommand

        public ICommand ChoosePathCommand { get; }

        private bool CanExecuteChoosePathCommand(object parameter) => true;

        private void OnChoosePathCommandExecuted(object parameter)
        {
            Log.Information("OnChoosePathCommandExecuted");

            Task.Run(() =>
            {
                var dialog = new CommonOpenFileDialog() { IsFolderPicker = true };
                CommonFileDialogResult result = Application.Current.Dispatcher.Invoke(dialog.ShowDialog);
                if (result != CommonFileDialogResult.Ok) return;

                OutputDirectoryPath = dialog.FileName;
            });
        }

        #endregion

        #region ChoosePathCommand

        public ICommand NavigateHomeCommand { get; }

        private bool CanExecuteNavigateHomeCommand(object parameter) => true;

        private void OnNavigateHomeCommandExecuted(object parameter)
        {
            Log.Information("OnNavigateHomeCommandExecuted");
            navigationService.Navigate<MainPage>();
        }

        #endregion

        private static readonly Account sampleAccount;
        static ExportPageViewModel()
        {
            sampleAccount = new Account("mitch13banks@gmail.com", "Sardine13");
            sampleAccount.UserId = 121211441;
            sampleAccount.Proxy = new Proxy(Socks4ProxyClient.Parse("81.18.34.98:47680"));
            sampleAccount.Token = "3761f993137551ac965ab71f8a4564305ddce3c8b2ecbcdac3bc30722cce4fa0";
            sampleAccount.RenewToken = "2f7bf7b922ccdfa091f4b66e30af996e8e06682921e02831e9589243014702ef";
            sampleAccount.ExpiresAt = DateTime.Now.AddDays(14);
        }

        private void UpdateSampleOutput()
        {
            Formatter.FormatScheme = ExportSettings.FormatScheme;
            OutputPreview = Formatter.Format(sampleAccount);
        }

        private void UpdateCanProceed()
            => CanProceed = Directory.Exists(OutputDirectoryPath)
            && !string.IsNullOrEmpty(ExportSettings.FormatScheme)
            && ExportSettings.Filters.Any(x => x.IsEnabled);

        private void UpdateSettingsRootPath()
        {
            if (string.IsNullOrEmpty(OutputDirectoryPath))
                ExportSettings.RootPath = null;
            else
            {
                ExportSettings.RootPath = OutputDirectoryPath +
                $"\\NVPNC {DateTime.Now:yyyy-MM-dd}" +
                $" at {DateTime.Now:HH-mm-ss}";
            }
        }

        #region Commands

        #region ExportCommand

        public ICommand ExportCommand { get; }

        private bool CanExecuteExportCommand(object parameter) => CanProceed;

        private void OnExportCommandExecuted(object parameter)
        {
            Log.Information("OnExportCommandExecuted");

            Task.Run(() =>
            {
                Log.Information("Exporting combos to {0}", ExportSettings.RootPath);
                Directory.CreateDirectory(ExportSettings.RootPath);
                Stopwatch watch = new Stopwatch();
                watch.Start();

                int counter = 0;
                foreach (var filter in ExportSettings.Filters)
                {
                    if (!filter.IsEnabled) continue;

                    IEnumerable<Account> selection = Accounts.Where(x => filter.Predicate(x));

                    string suffix = "";
                    if (ExportSettings.AreRowCountsAddedToFileNames)
                        suffix = $" ({selection.Count()})";
                    string fileName = filter.FileName.Replace("{suffix}", suffix);
                    using (StreamWriter sw = new StreamWriter(ExportSettings.RootPath + $"/{fileName}", true))
                    {
                        foreach (Account account in selection)
                        {
                            sw.WriteLine(Formatter.Format(account));
                            Log.Verbose("{0} has been saved to {1}", account, fileName);
                            counter++;
                        }
                    }
                }

                watch.Stop();
                Log.Information("{0} records have been exported to {1} in {2}ms",
                    counter, ExportSettings.RootPath, watch.ElapsedMilliseconds);
            });

            ExportSettings.CopyTo(App.ServiceProvider.GetService(typeof(ExportSettings)) as ExportSettings);
            navigationService.Navigate<MainPage>();
        }

        #endregion

        #endregion

        private void OnExportSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateCanProceed();
            if (e.PropertyName == nameof(ExportSettings.FormatScheme))
                UpdateSampleOutput();
        }

        public readonly System.Timers.Timer StateRefreshingTimer = new System.Timers.Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);

        public ExportPageViewModel(
            ObservableCollection<Account> accounts,
            NavigationService navigationService,
            ExportSettings exportSettings)
        {
            Accounts = accounts;
            this.navigationService = navigationService;
            ExportSettings = exportSettings.Clone() as ExportSettings;

            ExportSettings.PropertyChanged += OnExportSettingsPropertyChanged;
            if (ExportSettings.RootPath != null)
                OutputDirectoryPath = string.Join('\\', ExportSettings.RootPath.Split('\\').SkipLast(1));


            Formatter = new AccountFormatter();
            Formatter.AddPlaceholder("email", acc => acc.Email);
            Formatter.AddPlaceholder("password", acc => acc.Password);
            Formatter.AddPlaceholder("proxy", acc => acc.Proxy);
            Formatter.AddPlaceholder("expiration", acc => acc.ExpiresAt);
            Formatter.AddPlaceholder("services", acc => "<todo:services>");
            Formatter.AddPlaceholder("json", acc => "<todo:json>");

            UpdateSampleOutput();
            UpdateCanProceed();

            ExportCommand = new LambdaCommand(OnExportCommandExecuted, CanExecuteExportCommand);
            ChoosePathCommand = new LambdaCommand(OnChoosePathCommandExecuted, CanExecuteChoosePathCommand);
            NavigateHomeCommand = new LambdaCommand(OnNavigateHomeCommandExecuted, CanExecuteNavigateHomeCommand);

            StateRefreshingTimer.Elapsed += (sender, e) =>
            {
                UpdateSettingsRootPath();
                Log.Warning("local  ES RootPath set to {0}", ExportSettings.RootPath);
            };
            StateRefreshingTimer.Start();
        }

        ~ExportPageViewModel()
        {
            Log.Warning("~ExportPageViewModel");
            ExportSettings.PropertyChanged -= OnExportSettingsPropertyChanged;
        }
    }
}
