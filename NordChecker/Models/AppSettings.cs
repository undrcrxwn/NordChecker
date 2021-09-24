using HandyControl.Themes;
using Leaf.xNet;
using NordChecker.Shared;
using Serilog;
using Serilog.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NordChecker.Models
{
    public class ExportFilterParameter : INotifyPropertyChangedAdvanced, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _IsActivated;
        public bool IsActivated
        {
            get => _IsActivated;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsActivated, value, PropertyChanged, LogEventLevel.Information);
            }
        }

        private string _FileName;
        public string FileName
        {
            get => _FileName;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _FileName, value, PropertyChanged, LogEventLevel.Information);
            }
        }

        public ExportFilterParameter(bool isActivated, string directory)
        {
            IsActivated = isActivated;
            FileName = directory;

            PropertyChanged += (sender, e) =>
            {
                Log.Warning("some ExportFilterParameter' PropertyChanged");
                Log.Warning("{@0}", this);
            };
        }

        public object Clone()
        {
            ExportFilterParameter result = MemberwiseClone() as ExportFilterParameter;
            result.PropertyChanged = null;
            return result;
        }
    }

    public class ExportSettings : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableDictionary<AccountState, ExportFilterParameter> Filters { get; set; }

        private string _RootPath;
        public string RootPath
        {
            get => _RootPath;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _RootPath, value, PropertyChanged);
        }

        private string _FormatScheme = "{email}:{password} | {expiration} | {services}";
        public string FormatScheme
        {
            get => _FormatScheme;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _FormatScheme, value, PropertyChanged);
        }

        public ExportSettings()
        {

            var uncheckedExportParameter = new ExportFilterParameter(true, "Unchecked");
            Filters = new ObservableDictionary<AccountState, ExportFilterParameter>()
            {
                { AccountState.Premium,   new ExportFilterParameter(true, "Premium") },
                { AccountState.Free,      new ExportFilterParameter(true, "Free") },
                { AccountState.Invalid,   new ExportFilterParameter(true, "Invalid") },
                { AccountState.Unchecked, uncheckedExportParameter },
                { AccountState.Reserved,  uncheckedExportParameter }
            };

            SubscribeToFiltersChanged();
            //Filters.CollectionChanged +=
            //(object sender, NotifyCollectionChangedEventArgs e) =>
            //{
            //    var @this = this as INotifyPropertyChangedAdvanced;
            //    @this.OnPropertyChanged(
            //        PropertyChanged,
            //        nameof(Filters));

            //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Filters)));

            //    Log.Warning("ExportSettings object: Filters.CollectionChanged");
            //};
        }

        private void SubscribeToFiltersChanged()
        {
            foreach (var (state, parameter) in Filters)
            {
                if (state == AccountState.Reserved) continue;
                Log.Information("{0}\tsubscribed", state);
                parameter.PropertyChanged += (sender, e) =>
                {
                    var @this = this as INotifyPropertyChangedAdvanced;
                    @this.OnPropertyChanged(
                        PropertyChanged,
                        nameof(Filters));

                    (this as INotifyPropertyChangedAdvanced)
                    .OnPropertyChanged(PropertyChanged, nameof(Filters));

                    Log.Warning("ExportSettings object: parameter.PropertyChanged!!!");
                };
            }

            PropertyChanged += (sender, e) =>
            {
                Log.Warning("some ExportSettings' PropertyChanged");
                Log.Warning("{0}", Filters.Values.Select(x => x.IsActivated));
            };
        }

        public object Clone()
        {
            //var result = MemberwiseClone() as ExportSettings;
            ExportSettings result = MemberwiseClone() as ExportSettings;
            result.PropertyChanged = null;
            result.SubscribeToFiltersChanged();
            return result;
        }
    }

    public class AppSettings : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _IsDeveloperModeEnabled;
        public bool IsDeveloperModeEnabled
        {
            get => _IsDeveloperModeEnabled;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsDeveloperModeEnabled, value, PropertyChanged, LogEventLevel.Information);
            }
        }

        private bool _IsConsoleLoggingEnabled;
        public bool IsConsoleLoggingEnabled
        {
            get => _IsConsoleLoggingEnabled;
            set
            {
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
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _DataGridFilters, value, PropertyChanged, LogEventLevel.Information);
            }
        }

        private bool _AreComboDuplicatesSkipped = true;
        public bool AreComboDuplicatesSkipped
        {
            get => _AreComboDuplicatesSkipped;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreComboDuplicatesSkipped, value, PropertyChanged, LogEventLevel.Information);
            }
        }

        private bool _AreProxyDuplicatesSkipped = true;
        public bool AreProxyDuplicatesSkipped
        {
            get => _AreProxyDuplicatesSkipped;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreProxyDuplicatesSkipped, value, PropertyChanged, LogEventLevel.Information);
            }
        }

        private ProxyType _LastChosenProxyType = ProxyType.Socks4;
        public ProxyType LastChosenProxyType
        {
            get => _LastChosenProxyType;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _LastChosenProxyType, value, PropertyChanged, LogEventLevel.Information);
            }
        }

        private int _ThreadCount = 50;
        public int ThreadCount
        {
            get => _ThreadCount;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException();

                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ThreadCount, value, PropertyChanged, LogEventLevel.Information);
            }
        }

        private int _TimeoutInSeconds = 7;
        public int TimeoutInSeconds
        {
            get => _TimeoutInSeconds;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _TimeoutInSeconds, value, PropertyChanged, LogEventLevel.Information);
            }
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
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Theme, value, PropertyChanged, LogEventLevel.Information);
                ThemeManager.Current.ApplicationTheme = value;
            }
        }

        private SolidColorBrush _AccentColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#326cf3");
        public SolidColorBrush AccentColor
        {
            get => _AccentColor;
            set
            {
                if (_AccentColor.Color == value.Color) return;
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AccentColor, value, PropertyChanged, LogEventLevel.Information);
                ThemeManager.Current.AccentColor = value;
            }
        }

        private ExportSettings _ExportSettings = new ExportSettings();
        public ExportSettings ExportSettings
        {
            get => _ExportSettings;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ExportSettings, value, PropertyChanged, LogEventLevel.Information);
            }
        }

        public AppSettings()
        {
            _DataGridFilters = new ObservableDictionary<AccountState, bool>();
            foreach (AccountState key in Enum.GetValues(typeof(AccountState)))
                _DataGridFilters.Add(key, true);

            DataGridFilters.CollectionChanged +=
            (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                var @this = this as INotifyPropertyChangedAdvanced;
                @this.OnPropertyChanged(
                    PropertyChanged,
                    nameof(DataGridFilters));
                @this.LogPropertyChanged(
                    LogEventLevel.Information,
                    nameof(DataGridFilters),
                    DataGridFilters);
            };
        }
    }
}
