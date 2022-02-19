using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandyControl.Tools.Extension;
using Leaf.xNet;
using NordChecker.Infrastructure;
using NordChecker.Shared.Collections;

namespace NordChecker.Models.Stats
{
    public class ProxyStats : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableDictionary<ProxyState, int> _ByState;
        public ObservableDictionary<ProxyState, int> ByState
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

        private ObservableDictionary<ProxyType, int> _ByType;
        public ObservableDictionary<ProxyType, int> ByType
        {
            get => _ByType;
            set
            {
                if (_ByType is not null)
                    _ByType.CollectionChanged -= OnByStateCollectionChanged;

                (this as INotifyPropertyChangedAdvanced)
                    .Set(ref _ByType, value, PropertyChanged);

                _ByType.CollectionChanged += OnByStateCollectionChanged;
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

        public ProxyStats()
        {
            var proxyStateDictionary = Enum.GetValues<ProxyState>()
                .ToDictionary(key => key, value => 0);
            ByState = new ObservableDictionary<ProxyState, int>(proxyStateDictionary);

            var proxyTypeDictionary = Enum.GetValues<ProxyType>().Reverse()
                .ToDictionary(key => key, value => 0);
            ByType = new ObservableDictionary<ProxyType, int>(proxyTypeDictionary);
        }

        public void Clear()
        {
            ByState.ForEach(x => ByState[x.Key] = 0);
            ByType.ForEach(x => ByType[x.Key] = 0);
            DuplicatesCount = 0;
            MismatchedCount = 0;
        }

        private void OnByStateCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            (this as INotifyPropertyChangedAdvanced)
                .OnPropertyChanged(PropertyChanged, nameof(ByType));
        }
    }
}
