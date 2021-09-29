using NordChecker.Models;
using NordChecker.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NordChecker.ViewModels
{
    public class ComboBaseViewModel : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Account> _Accounts = new ObservableCollection<Account>();
        public ObservableCollection<Account> Accounts
        {
            get => _Accounts;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Accounts, value, PropertyChanged);
        }

        private int _LoadedCount;
        public int LoadedCount
        {
            get => _LoadedCount;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _LoadedCount, value, PropertyChanged);
        }

        private int _MismatchedCount;
        public int MismatchedCount
        {
            get => _MismatchedCount;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _MismatchedCount, value, PropertyChanged);
        }

        private int _DuplicatesCount;
        public int DuplicatesCount
        {
            get => _DuplicatesCount;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _DuplicatesCount, value, PropertyChanged);
        }

       

        

        public ComboBaseViewModel()
        {
            
        }
    }
}
