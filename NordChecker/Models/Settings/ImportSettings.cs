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
    public class ImportSettings : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Combos

        private bool _AreComboDuplicatesSkipped;
        public bool AreComboDuplicatesSkipped
        {
            get => _AreComboDuplicatesSkipped;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreComboDuplicatesSkipped, value, PropertyChanged, LogEventLevel.Information);
        }

        private bool _AreComboLinesNormalized;
        public bool AreComboLinesNormalized
        {
            get => _AreComboLinesNormalized;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreComboLinesNormalized, value, PropertyChanged, LogEventLevel.Information);
        }

        private string _ComboRegexMask = @"\W*(^\w+(?:[-+.']\w+|-)*@\w+(?:[-.]\w+)*\.\w+(?:[-.]\w+)*):(\w+)\W*$";
        public string ComboRegexMask
        {
            get => _ComboRegexMask;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ComboRegexMask, value, PropertyChanged, LogEventLevel.Information);
        }

        #endregion

        #region Proxies

        private string _ProxyImportDirectory;
        public string ProxyImportDirectory
        {
            get => _ProxyImportDirectory;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ProxyImportDirectory, value, PropertyChanged);
        }

        private bool _AreProxyDuplicatesSkipped;
        public bool AreProxyDuplicatesSkipped
        {
            get => _AreProxyDuplicatesSkipped;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreProxyDuplicatesSkipped, value, PropertyChanged, LogEventLevel.Information);
        }

        private bool _AreProxyLinesNormalized;
        public bool AreProxyLinesNormalized
        {
            get => _AreProxyLinesNormalized;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreProxyLinesNormalized, value, PropertyChanged, LogEventLevel.Information);
        }

        private string _ProxyRegexMask = @"([\d.]+):(\d+)";
        public string ProxyRegexMask
        {
            get => _ProxyRegexMask;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ProxyRegexMask, value, PropertyChanged, LogEventLevel.Information);
        }

        private ProxyType _ProxyType = ProxyType.Socks4;
        public ProxyType ProxyType
        {
            get => _ProxyType;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ProxyType, value, PropertyChanged, LogEventLevel.Information);
        }

        #endregion
    }
}
