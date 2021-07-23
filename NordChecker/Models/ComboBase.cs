using NordChecker.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NordChecker.Models
{
    internal enum ComboBaseState
    {
        Idle,
        Processing
    }

    internal class ComboBase : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Account> _Accounts = new ObservableCollection<Account>();
        public ObservableCollection<Account> Accounts
        {
            get => _Accounts;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Accounts, value, PropertyChanged);
        }

        public Dictionary<AccountState, int> CalculateStats()
        {
            var result = new Dictionary<AccountState, int>();
            foreach (AccountState accountState in Enum.GetValues(typeof(AccountState)))
                result[accountState] = Accounts.Count(a => a.State == accountState);
            return result;
        }
    }
}
