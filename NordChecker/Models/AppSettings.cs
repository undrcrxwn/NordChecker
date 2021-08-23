using NordChecker.Shared;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public class AppSettings : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool _IsDeveloperModeOn;
        public bool IsDeveloperModeEnabled
        {
            get => _IsDeveloperModeOn;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsDeveloperModeOn, value, PropertyChanged, LogEventLevel.Information);
            }
        }

        public bool _AreComboDuplicatesSkipped = true;
        public bool AreComboDuplicatesSkipped
        {
            get => _AreComboDuplicatesSkipped;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreComboDuplicatesSkipped, value, PropertyChanged, LogEventLevel.Information);
            }
        }

        public int _ThreadCount = 50;
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

        public int _TimeoutInSeconds= 7;
        public int TimeoutInSeconds
        {
            get => _TimeoutInSeconds;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _TimeoutInSeconds, value, PropertyChanged, LogEventLevel.Information);
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

        public LogEventLevel _LogEventLevel = LogEventLevel.Information;
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
    }
}
