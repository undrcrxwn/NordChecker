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

        private ObservableDictionary<ProxyType, int> _ValidByType;
        public ObservableDictionary<ProxyType, int> ValidByType
        {
            get => _ValidByType;
            set
            {
                if (_ValidByType is not null)
                    _ValidByType.CollectionChanged -= OnValidByTypeCollectionChanged;

                (this as INotifyPropertyChangedAdvanced)
                    .Set(ref _ValidByType, value, PropertyChanged);

                _ValidByType.CollectionChanged += OnValidByTypeCollectionChanged;
            }
        }

        private int _InvalidCount;
        public int InvalidCount
        {
            get => _InvalidCount;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _InvalidCount, value, PropertyChanged);
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
            var dictionary = Enum.GetValues<ProxyType>().Reverse()
                .ToDictionary(x => x, _ => 0);
            ValidByType = new ObservableDictionary<ProxyType, int>(dictionary);
        }

        public void SynchronizeWith(Cyclic<Proxy> proxies)
        {
            lock (proxies)
            {
                foreach (ProxyType proxyType in Enum.GetValues(typeof(ProxyType)))
                {
                    ValidByType[proxyType] = proxies.FiniteCollection.Count(x =>
                        x.State == ProxyState.Valid && x.Client.Type == proxyType);
                }

                InvalidCount = proxies.Count - ValidByType.Sum(x => x.Value);
            }
        }

        public void Clear()
        {
            ValidByType.ForEach(x =>
                ValidByType[x.Key] = 0);
            InvalidCount = 0;
            DuplicatesCount = 0;
            MismatchedCount = 0;
        }

        private void OnValidByTypeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            (this as INotifyPropertyChangedAdvanced)
                .OnPropertyChanged(PropertyChanged, nameof(ValidByType));
        }
    }
}
