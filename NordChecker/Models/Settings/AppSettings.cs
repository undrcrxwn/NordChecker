using HandyControl.Themes;
using Leaf.xNet;
using NordChecker.Shared;
using Serilog;
using Serilog.Events;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using NordChecker.Infrastructure;
using NordChecker.Shared.Collections;

namespace NordChecker.Models.Settings
{
    public class AppSettings : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _IsDeveloperModeEnabled;
        public bool IsDeveloperModeEnabled
        {
            get => _IsDeveloperModeEnabled;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsDeveloperModeEnabled, value, PropertyChanged, LogEventLevel.Information);
        }

        private bool _IsConsoleLoggingEnabled;
        public bool IsConsoleLoggingEnabled
        {
            get => _IsConsoleLoggingEnabled;
            set
            {
                if (_IsConsoleLoggingEnabled == value) return;

                if (value)
                {
                    Utils.ShowConsole();
                    Log.Logger = Log.Logger.Merge(App.ConsoleLogger);
                    (this as INotifyPropertyChangedAdvanced)
                        .Set(ref _IsConsoleLoggingEnabled, value, PropertyChanged, LogEventLevel.Information);
                }
                else
                {
                    (this as INotifyPropertyChangedAdvanced)
                        .Set(ref _IsConsoleLoggingEnabled, value, PropertyChanged, LogEventLevel.Information);
                    Utils.HideConsole();
                    Log.Logger = App.FileLogger;
                }
            }
        }

        private ObservableDictionary<AccountState, bool> _DataGridFilters;
        public ObservableDictionary<AccountState, bool> DataGridFilters
        {
            get => _DataGridFilters;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _DataGridFilters, value, PropertyChanged, LogEventLevel.Information);
        }

        private bool _AreComboDuplicatesSkipped;
        public bool AreComboDuplicatesSkipped
        {
            get => _AreComboDuplicatesSkipped;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreComboDuplicatesSkipped, value, PropertyChanged, LogEventLevel.Information);
        }

        private bool _AreProxyDuplicatesSkipped;
        public bool AreProxyDuplicatesSkipped
        {
            get => _AreProxyDuplicatesSkipped;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreProxyDuplicatesSkipped, value, PropertyChanged, LogEventLevel.Information);
        }

        private string _ComboRegexMask = @"\W*(^\w+(?:[-+.']\w+|-)*@\w+(?:[-.]\w+)*\.\w+(?:[-.]\w+)*):(\w+)\W*$";
        public string ComboRegexMask
        {
            get => _ComboRegexMask;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ComboRegexMask, value, PropertyChanged, LogEventLevel.Information);
        }

        private string _ProxyRegexMask;
        public string ProxyRegexMask
        {
            get => _ProxyRegexMask;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ProxyRegexMask, value, PropertyChanged, LogEventLevel.Information);
        }

        private ProxyType _LastChosenProxyType = ProxyType.Socks4;
        public ProxyType LastChosenProxyType
        {
            get => _LastChosenProxyType;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _LastChosenProxyType, value, PropertyChanged, LogEventLevel.Information);
        }
        
        private int _ThreadCount = 50;
        public int ThreadCount
        {
            get => _ThreadCount;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ThreadCount, value, PropertyChanged, LogEventLevel.Information);
        }

        private TimeSpan _Timeout = TimeSpan.FromSeconds(5);
        public TimeSpan Timeout
        {
            get => _Timeout;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Timeout, value, PropertyChanged, LogEventLevel.Information);
        }

        private LogEventLevel _LogEventLevel = LogEventLevel.Information;
        public LogEventLevel LogEventLevel
        {
            get => _LogEventLevel;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                    .Set(ref _LogEventLevel, value, PropertyChanged, LogEventLevel.Information);
                App.LogLevelSwitch.MinimumLevel = value;
            }
        }

        private ApplicationTheme _Theme = ApplicationTheme.Light;
        public ApplicationTheme Theme
        {
            get => _Theme;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Theme, value, PropertyChanged, LogEventLevel.Information);
        }

        private SolidColorBrush _AccentColor = new BrushConverter().ConvertFrom("#657CF8") as SolidColorBrush;
        public SolidColorBrush AccentColor
        {
            get => _AccentColor;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AccentColor, value, PropertyChanged, LogEventLevel.Information);
        }

        private bool _IsTopMostWindow;
        public bool IsTopMostWindow
        {
            get => _IsTopMostWindow;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsTopMostWindow, value, PropertyChanged, LogEventLevel.Information);
        }

        private bool _IsMinimizedToTray;
        public bool IsMinimizedToTray
        {
            get => _IsMinimizedToTray;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsMinimizedToTray, value, PropertyChanged, LogEventLevel.Information);
        }

        private bool _IsAutoSaveEnabled = true;
        public bool IsAutoSaveEnabled
        {
            get => _IsAutoSaveEnabled;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsAutoSaveEnabled, value, PropertyChanged, LogEventLevel.Information);
        }

        private TimeSpan _ContinuousSyncInterval = TimeSpan.FromSeconds(30);
        public TimeSpan ContinuousSyncInterval
        {
            get => _ContinuousSyncInterval;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ContinuousSyncInterval, value, PropertyChanged, LogEventLevel.Information);
        }

        public AppSettings()
        {
            var dictionary = Enum.GetValues<AccountState>().Reverse()
                .ToDictionary(key => key, value => true);
            DataGridFilters = new ObservableDictionary<AccountState, bool>(dictionary);
        }
    }
}
