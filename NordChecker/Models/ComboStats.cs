using NordChecker.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public class ComboStats : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableDictionary<AccountState, int> _ByState;
        public ObservableDictionary<AccountState, int> ByState
        {
            get => _ByState;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ByState, value, PropertyChanged);
        }

        private int _DuplicatesCount;
        public int DuplicatesCount
        {
            get => _DuplicatesCount;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _DuplicatesCount, value, PropertyChanged);
        }
        
        private int _MismatchedCount;
        public int MismatchedCount
        {
            get => _MismatchedCount;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _MismatchedCount, value, PropertyChanged);
        }

        public ComboStats()
        {
            ByState = new ObservableDictionary<AccountState, int>();
            Clear();

            ByState.CollectionChanged += (sender, e) =>
                (this as INotifyPropertyChangedAdvanced)
                .OnPropertyChanged(PropertyChanged, nameof(ByState));
        }

        public void Clear()
        {
            foreach (AccountState key in Enum.GetValues(typeof(AccountState)))
                ByState[key] = 0;
            DuplicatesCount = 0;
            MismatchedCount = 0;
        }
    }
}
