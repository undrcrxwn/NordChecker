using NordChecker.Shared;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;
using System.Reflection;
using HandyControl.Tools;

namespace NordChecker.Models
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

        public object Clone()
        {
            var copy = MemberwiseClone() as OutputFilter<TPayload>;
            copy.PropertyChanged = null;
            return copy;
        }
    }

    [JsonObject]
    public class AccountFilters : INotifyPropertyChangedAdvanced, IEnumerable<OutputFilter<Account>>
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private OutputFilter<Account> _Premium = new(
            "Premium{suffix}.txt",
            x => x.State == AccountState.Premium);
        public OutputFilter<Account> Premium
        {
            get => _Premium;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Premium, value, PropertyChanged);
        }
        
        private OutputFilter<Account> _Free = new(
            "Free{suffix}.txt",
            x => x.State == AccountState.Free);
        public OutputFilter<Account> Free
        {
            get => _Free;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Free, value, PropertyChanged);
        }
        
        private OutputFilter<Account> _Invalid = new(
            "Invalid{suffix}.txt",
            x => x.State == AccountState.Invalid);
        public OutputFilter<Account> Invalid
        {
            get => _Invalid;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Invalid, value, PropertyChanged);
        }
        
        private OutputFilter<Account> _UncheckedAndReserved = new(
            "Unchecked{suffix}.txt",
            x => x.State == AccountState.Unchecked || x.State == AccountState.Reserved);
        public OutputFilter<Account> UncheckedAndReserved
        {
            get => _UncheckedAndReserved;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _UncheckedAndReserved, value, PropertyChanged);
        }
        
        public IEnumerator<OutputFilter<Account>> GetEnumerator()
        {
            return typeof(AccountFilters).GetProperties()
                .Where(x => x.PropertyType == typeof(OutputFilter<Account>))
                .Select(x => x.GetValue(this, null))
                .GetEnumerator() as IEnumerator<OutputFilter<Account>>;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
                .Set(ref _RootPath, value, PropertyChanged, LogEventLevel.Warning);
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
        
        public object Clone()
        {
            var copy = new ExportSettings();
            CopyTo(copy);
            return copy;
        }

        public void CopyTo(ExportSettings target)
        {
            target.Filters = Filters;
            target.RootPath = RootPath;
            target.FormatScheme = FormatScheme;
            target.AreRowCountsAddedToFileNames = AreRowCountsAddedToFileNames;
        }
    }
}
