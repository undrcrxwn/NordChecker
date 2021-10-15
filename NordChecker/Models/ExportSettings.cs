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

namespace NordChecker.Models
{
    public class AccountFilter : INotifyPropertyChangedAdvanced, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        public Predicate<Account> Predicate;

        private bool _IsEnabled;
        public bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsEnabled, value, PropertyChanged, LogEventLevel.Information);
            }
        }

        public AccountFilter(string fileName, Predicate<Account> predicate, bool isEnabled = true)
        {
            FileName = fileName;
            Predicate = predicate;
            IsEnabled = isEnabled;
        }

        public object Clone()
        {
            var copy = MemberwiseClone() as AccountFilter;
            copy.PropertyChanged = null;
            return copy;
        }
    }

    public class ExportSettings : INotifyPropertyChangedAdvanced, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<AccountFilter> _AccountFilters;
        public ObservableCollection<AccountFilter> AccountFilters
        {
            get => _AccountFilters;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _AccountFilters, value, PropertyChanged);
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

        public ExportSettings()
        {
            AccountFilters = new ObservableCollection<AccountFilter>()
            {
                new AccountFilter("Premium{suffix}.txt", x => x.State == AccountState.Premium),
                new AccountFilter("Free{suffix}.txt", x => x.State == AccountState.Free),
                new AccountFilter("Invalid{suffix}.txt", x => x.State == AccountState.Premium),
                new AccountFilter("Unchecked{suffix}.txt", x => x.State == AccountState.Unchecked || x.State == AccountState.Reserved)
            };
            SubscribeToFiltersChanged();
        }

        ~ExportSettings() => UnsubscribeFromFiltersChanged();

        public object Clone()
        {
            ExportSettings copy = new ExportSettings();
            CopyTo(copy);
            return copy;
        }

        public void CopyTo(ExportSettings target)
        {
            target.UnsubscribeFromFiltersChanged();
            target.AccountFilters = new ObservableCollection<AccountFilter>(AccountFilters.Select(x => x.Clone() as AccountFilter));
            target.SubscribeToFiltersChanged();
            target.RootPath = RootPath;
            target.FormatScheme = FormatScheme;
            target.AreRowCountsAddedToFileNames = AreRowCountsAddedToFileNames;
        }

        private void OnFilterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            (this as INotifyPropertyChangedAdvanced)
            .OnPropertyChanged(PropertyChanged, nameof(AccountFilters));
        }

        private void SubscribeToFiltersChanged()
        {
            foreach (AccountFilter filter in AccountFilters)
                filter.PropertyChanged += OnFilterPropertyChanged;
        }

        private void UnsubscribeFromFiltersChanged()
        {
            foreach (AccountFilter filter in AccountFilters)
                filter.PropertyChanged -= OnFilterPropertyChanged;
        }
    }
}
