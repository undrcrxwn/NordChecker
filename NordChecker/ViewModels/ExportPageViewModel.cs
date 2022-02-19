using Leaf.xNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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
using System.Windows.Threading;
using NordChecker.Models.Settings;
using NordChecker.Infrastructure;
using NordChecker.Services;
using NordChecker.Services.AccountFormatter;
using Prism.Commands;
using Prism.Mvvm;

namespace NordChecker.ViewModels
{
    public partial class ExportPageViewModel : BindableBase, IPageViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private NavigationService navigationService;

        #region Properties

        private string _Title;
        public string Title
        {
            get => _Title;
            protected set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Title, value, PropertyChanged);
        }

        private string _Description;
        public string Description
        {
            get => _Description;
            protected set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Description, value, PropertyChanged);
        }

        private ObservableCollection<Account> _Accounts;
        public ObservableCollection<Account> Accounts
        {
            get => _Accounts;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Accounts, value, PropertyChanged);
        }
        
        private Wrapped<AppSettings> _AppSettingsWrapped;
        public Wrapped<AppSettings> AppSettingsWrapped
        {
            get => _AppSettingsWrapped;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AppSettingsWrapped, value, PropertyChanged);
        }

        private Wrapped<ExportSettings> _ExportSettingsWrapped;

        private ExportSettings _ExportSettingsDraft;
        public ExportSettings ExportSettingsDraft
        {
            get => _ExportSettingsDraft;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ExportSettingsDraft, value, PropertyChanged);
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









        private int _Hash;
        public int Hash
        {
            get => _Hash;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                    .Set(ref _Hash, value, PropertyChanged);
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

        private bool _CanProceed;
        public bool CanProceed
        {
            get => _CanProceed;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _CanProceed, value, PropertyChanged);
        }

        #endregion

        private static readonly Account sampleAccount;
        static ExportPageViewModel()
        {
            sampleAccount = new Account("mitch13banks@gmail.com", "Sardine13")
            {
                UserId = 120161441,
                Proxy = new Proxy(Socks4ProxyClient.Parse("81.18.34.98:47680")),
                Token = "3761f993137551ac965ab71f8a4564305ddce3c8b2ecbcdac3bc30722cce4fa0",
                RenewToken = "2f7bf7b922ccdfa091f4b66e30af996e8e06682921e02831e9589243014702ef",
                ExpiresAt = DateTime.Now.AddDays(14)
            };
        }

        private void UpdateOutputPreview()
        {
            OutputPreview = Formatter.Format(sampleAccount);
            Log.Warning(ExportSettingsDraft.FormatScheme);
        }

        private void UpdateCanProceed()
        {
            CanProceed = Directory.Exists(OutputDirectoryPath)
                         && !string.IsNullOrEmpty(ExportSettingsDraft.FormatScheme)
                         && ExportSettingsDraft.Filters.Any(x => x.IsEnabled);
            Log.Warning(nameof(UpdateCanProceed));
        }

        private void UpdateSettingsRootPath()
        {
            if (string.IsNullOrEmpty(OutputDirectoryPath))
                ExportSettingsDraft.RootPath = null;
            else
            {
                ExportSettingsDraft.RootPath = OutputDirectoryPath +
                $"\\NVPNC {DateTime.Now:yyyy-MM-dd}" +
                $" at {DateTime.Now:HH-mm-ss}";
            }
        }

        #region Commands

        #region ExportCommand

        public ICommand ExportCommand { get; }

        private bool CanExecuteExportCommand() => CanProceed;

        private void OnExportCommandExecuted()
        {
            Log.Information("OnExportCommandExecuted");

            Task.Run(() =>
            {
                Log.Information("Exporting combos to {0}", ExportSettingsDraft.RootPath);
                Directory.CreateDirectory(ExportSettingsDraft.RootPath);
                Stopwatch watch = new Stopwatch();
                watch.Start();

                Formatter.FormatScheme = ExportSettingsDraft.FormatScheme;

                int counter = 0;
                foreach (var filter in ExportSettingsDraft.Filters)
                {
                    if (!filter.IsEnabled) continue;

                    IEnumerable<Account> selection = Accounts.Where(x => filter.Predicate(x));

                    string suffix = "";
                    if (ExportSettingsDraft.AreRowCountsAddedToFileNames)
                        suffix = $" ({selection.Count()})";
                    string fileName = filter.FileName.Replace("{suffix}", suffix);
                    using (var writer = new StreamWriter(ExportSettingsDraft.RootPath + $"/{fileName}", true))
                    {
                        foreach (var account in selection)
                        {
                            writer.WriteLine(Formatter.Format(account));
                            Log.Verbose("{0} has been saved to {1}", account, fileName);
                            counter++;
                        }
                    }
                }

                watch.Stop();
                Log.Information("{0} records have been exported to {1} in {2}ms",
                    counter, ExportSettingsDraft.RootPath, watch.ElapsedMilliseconds);
            });

            _ExportSettingsWrapped.ReplaceWith(ExportSettingsDraft);
            navigationService.NavigateContent("MainView");
        }

        #endregion

        #endregion

        private void OnExportSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateCanProceed();
            Log.Warning("OnExportSettingsPropertyChanged");
            if (e.PropertyName == nameof(ExportSettingsDraft.FormatScheme))
            {
                Formatter.FormatScheme = ExportSettingsDraft.FormatScheme;
                UpdateOutputPreview();
            }
        }

        public readonly System.Timers.Timer StateRefreshingTimer = new(TimeSpan.FromSeconds(1).TotalMilliseconds);

        public ExportPageViewModel(
            ObservableCollection<Account> accounts,
            NavigationService navigationService,
            Wrapped<AppSettings> appSettingsWrapped,
            Wrapped<ExportSettings> exportSettingsWrapped)
        {
            Accounts = accounts;
            this.navigationService = navigationService;
            AppSettingsWrapped = appSettingsWrapped;
            _ExportSettingsWrapped = exportSettingsWrapped;
            ExportSettingsDraft = _ExportSettingsWrapped.Instance.Clone();
            Hash = ExportSettingsDraft.GetHashCode();

            Log.Warning("EXPORT SETTINGS INSTANCE USED BY VM = {0}", ExportSettingsDraft.GetHashCode());

            Log.Warning("AFTER ES CLONED ExportPageViewModel c-tor");
            Log.Warning("SETTINGS = {0}, FILTERS = {1}, PREMIUM = {2}",
                ExportSettingsDraft.GetHashCode(), ExportSettingsDraft.Filters.GetHashCode(), ExportSettingsDraft.Filters.Premium.GetHashCode());

            Formatter = new AccountFormatter(ExportSettingsDraft.FormatScheme);

            ExportSettingsDraft.PropertyChanged += OnExportSettingsPropertyChanged;
            if (ExportSettingsDraft.RootPath is not null)
                OutputDirectoryPath = string.Join('\\', ExportSettingsDraft.RootPath.Split('\\').SkipLast(1));

            UpdateOutputPreview();
            UpdateCanProceed();

            ExportCommand = new DelegateCommand(OnExportCommandExecuted, CanExecuteExportCommand)
                .ObservesProperty(() => CanProceed);
            ExportCommand.CanExecuteChanged += (sender, e) => Log.Warning("event raised");

            ChoosePathCommand = new DelegateCommand(OnChoosePathCommandExecuted);
            NavigateHomeCommand = new DelegateCommand(OnNavigateHomeCommandExecuted);

            StateRefreshingTimer.Elapsed += (sender, e) => UpdateSettingsRootPath();
            StateRefreshingTimer.Start();
            //StateRefreshingTimer.Stop();

            ExportSettingsDraft.PropertyChanged += (sender, e) => Log.Warning("STH CHANGED");
        }
    }
}
