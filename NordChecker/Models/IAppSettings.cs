using HandyControl.Themes;
using Leaf.xNet;
using NordChecker.Shared;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NordChecker.Models
{
    public interface IAppSettings : INotifyPropertyChangedAdvanced
    {
        public bool IsDeveloperModeEnabled { get; set; }
        public bool IsConsoleLoggingEnabled { get; set; }
        public ObservableDictionary<AccountState, bool> DataGridFilters { get; set; }
        public bool AreComboDuplicatesSkipped { get; set; }
        public bool AreProxyDuplicatesSkipped { get; set; }
        public ProxyType LastChosenProxyType { get; set; }
        public int ThreadCount { get; set; }
        public int TimeoutInSeconds { get; set; }
        public string ExportFormatScheme { get; set; }
        public LogEventLevel LogEventLevel { get; set; }
        public ApplicationTheme Theme { get; set; }
        public SolidColorBrush AccentColor { get; set; }
    }
}
