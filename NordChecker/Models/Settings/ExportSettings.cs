using Newtonsoft.Json;
using NordChecker.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using NordChecker.Infrastructure;
using Serilog;

namespace NordChecker.Models.Settings
{
    public class OutputFilter<TPayload> : INotifyPropertyChangedAdvanced, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public Predicate<TPayload> Predicate;

        private string _FileName;
        public string FileName
        {
            get => _FileName;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _FileName, value, PropertyChanged);
        }
        
        private bool _IsEnabled;
        public bool IsEnabled
        {
            get => _IsEnabled;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsEnabled, value, PropertyChanged);
        }
        
        public OutputFilter(string fileName, Predicate<TPayload> predicate, bool isEnabled = true)
        {
            FileName = fileName;
            Predicate = predicate;
            IsEnabled = isEnabled;
        }

        public OutputFilter<TPayload> Clone()
        {
            Log.Warning("public OutputFilter<TPayload> Clone()");
            var copy = MemberwiseClone() as OutputFilter<TPayload>;
            copy.PropertyChanged = null;
            return copy;
        }

        object ICloneable.Clone() => Clone();
    }

    [JsonObject]
    public class AccountFilters : INotifyPropertyChangedAdvanced, ICloneable, IEnumerable<OutputFilter<Account>>
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private OutputFilter<Account> _Premium = new(
            "Premium{suffix}.txt", x => x.State == AccountState.Premium);
        public OutputFilter<Account> Premium
        {
            get => _Premium;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Premium, value, PropertyChanged);
        }
        
        private OutputFilter<Account> _Free = new(
            "Free{suffix}.txt", x => x.State == AccountState.Free);
        public OutputFilter<Account> Free
        {
            get => _Free;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Free, value, PropertyChanged);
        }
        
        private OutputFilter<Account> _Invalid = new(
            "Invalid{suffix}.txt", x => x.State == AccountState.Invalid);
        public OutputFilter<Account> Invalid
        {
            get => _Invalid;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Invalid, value, PropertyChanged);
        }
        
        private OutputFilter<Account> _UncheckedAndReserved = new(
            "Unchecked{suffix}.txt", x => x.State == AccountState.Unchecked || x.State == AccountState.Reserved);
        public OutputFilter<Account> UncheckedAndReserved
        {
            get => _UncheckedAndReserved;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _UncheckedAndReserved, value, PropertyChanged);
        }

        public AccountFilters()
        {
            SubscribeToFilters();
        }
        
        public IEnumerator<OutputFilter<Account>> GetEnumerator() =>
            GetFilterProperties().Select(x => x.GetValue(this, null) as OutputFilter<Account>).GetEnumerator();

        private IEnumerable<PropertyInfo> GetFilterProperties() =>
            typeof(AccountFilters).GetProperties().Where(x => x.PropertyType == typeof(OutputFilter<Account>));

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public AccountFilters Clone()
        {
            var copy = (AccountFilters)MemberwiseClone();
            copy.PropertyChanged = null;

            copy.Premium = Premium.Clone();
            copy.Free = Free.Clone();
            copy.Invalid = Invalid.Clone();
            copy.UncheckedAndReserved = UncheckedAndReserved.Clone();

            copy.SubscribeToFilters();

            Log.Warning("public AccountFilters Clone()");
            return copy;
        }

        object ICloneable.Clone() => Clone();

        private void SubscribeToFilters()
        {
            foreach (PropertyInfo property in GetFilterProperties())
            {
                var filter = (OutputFilter<Account>)property.GetValue(this, null);
                filter.PropertyChanged += (sender, e) =>
                {
                    (this as INotifyPropertyChangedAdvanced)
                        .OnPropertyChanged(PropertyChanged, nameof(property.Name));
                    Log.Warning("some filter changed");
                };
            }
        }
    }
    
    public class ExportSettings : INotifyPropertyChangedAdvanced, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private AccountFilters _Filters = new();
        public AccountFilters Filters
        {
            get => _Filters;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Filters, value, PropertyChanged);
        }

        private string _RootPath;
        public string RootPath
        {
            get => _RootPath;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _RootPath, value, PropertyChanged);
        }

        private string _FormatScheme = "{email}:{password} | {proxy} | {expiration}";
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
            Filters.PropertyChanged += OnFiltersPropertyChanged;
            
            Log.Warning("public ExportSettings c-tor");
            Log.Warning("SETTINGS = {0}, FILTERS = {1}, PREMIUM = {2}",
                GetHashCode(), Filters.GetHashCode(), Filters.Premium.GetHashCode());
        }

        public void OnFiltersPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            {
                (this as INotifyPropertyChangedAdvanced)
                    .OnPropertyChanged(PropertyChanged, nameof(Filters));
                Log.Warning("{0} ExportSettings event: Filters.PropertyChanged!!", this.GetHashCode());
            };
        }

        public ExportSettings Clone()
        {
            var copy = (ExportSettings)MemberwiseClone();
            copy.PropertyChanged = (sender, e) =>
            {
                //if (e.PropertyName != nameof(Filters))
                //(this as INotifyPropertyChangedAdvanced)
                //    .OnPropertyChanged(PropertyChanged, nameof(Filters));
                Log.Warning("{0} ExportSettings copy event: {1} PropertyChanged", this.GetHashCode(), e.PropertyName);
            };
            copy.Filters = Filters.Clone();
            copy.Filters.PropertyChanged += OnFiltersPropertyChanged;

            Log.Warning("public ExportSettings Clone()");
            Log.Warning("COPY HASH = {0}", copy.GetHashCode());

            Log.Warning("public ExportSettings Clone()");
            Log.Warning("SETTINGS = {0}, FILTERS = {1}, PREMIUM = {2}",
                copy.GetHashCode(), copy.Filters.GetHashCode(), copy.Filters.Premium.GetHashCode());
            return copy;
        }

        object ICloneable.Clone() => Clone();
    }
}
