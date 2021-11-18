using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using HandyControl.Tools.Extension;
using NordChecker.Infrastructure;
using NordChecker.Shared.Collections;

namespace NordChecker.Models
{
    public class ComboStats : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableDictionary<AccountState, int> _ByState;
        public ObservableDictionary<AccountState, int> ByState
        {
            get => _ByState;
            set
            {
                if (_ByState is not null)
                    _ByState.CollectionChanged -= OnByStateCollectionChanged;

                (this as INotifyPropertyChangedAdvanced)
                    .Set(ref _ByState, value, PropertyChanged);

                _ByState.CollectionChanged += OnByStateCollectionChanged;
            }
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
            var dictionary = Enum.GetValues<AccountState>().Reverse()
                .ToDictionary(key => key, value => 0);
            ByState = new ObservableDictionary<AccountState, int>(dictionary);
        }

        public void Clear()
        {
            ByState.ForEach(x => ByState[x.Key] = 0);
            DuplicatesCount = 0;
            MismatchedCount = 0;
        }

        private void OnByStateCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            (this as INotifyPropertyChangedAdvanced)
                .OnPropertyChanged(PropertyChanged, nameof(ByState));
        }
    }
}
