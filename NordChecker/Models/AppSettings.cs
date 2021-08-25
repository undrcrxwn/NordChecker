﻿using HandyControl.Themes;
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
using System.Windows.Media;

namespace NordChecker.Models
{
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

        private Brush _AccentColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#326cf3");
        public Brush AccentColor
        {
            get => _AccentColor;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AccentColor, value, PropertyChanged, LogEventLevel.Information);
                ThemeManager.Current.AccentColor = value;
            }
        }
    
    
    }
}
