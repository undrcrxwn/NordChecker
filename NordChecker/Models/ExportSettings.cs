using NordChecker.Shared;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public object Clone()
        {
            var result = MemberwiseClone() as ExportFilterParameter;
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

        private bool _AreRowCountsAddedToFileNames = true;
        public bool AreRowCountsAddedToFileNames
        {
            get => _AreRowCountsAddedToFileNames;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AreRowCountsAddedToFileNames, value, PropertyChanged);
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
        }

        public object Clone()
        {
            ExportSettings result = MemberwiseClone() as ExportSettings;
            var dictionary = Filters.ToDictionary(x => x.Key, x => x.Value.Clone() as ExportFilterParameter);
            result.Filters = new ObservableDictionary<AccountState, ExportFilterParameter>(dictionary);

            result.PropertyChanged = null;
            result.SubscribeToFiltersChanged();

            return result;
        }

        private void SubscribeToFiltersChanged()
        {
            foreach (var (state, parameter) in Filters)
            {
                if (state == AccountState.Reserved) continue;
                Log.Debug("{0} subscribed", state);
                parameter.PropertyChanged += (sender, e) =>
                {
                    (this as INotifyPropertyChangedAdvanced)
                    .OnPropertyChanged(PropertyChanged, nameof(Filters));
                };
            }

            Filters[AccountState.Unchecked].PropertyChanged += (sender, e)
                => Filters[AccountState.Reserved] = Filters[AccountState.Unchecked];
        }
    }
}
