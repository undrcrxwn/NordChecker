using Leaf.xNet;
using NordChecker.Models;
using NordChecker.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NordChecker.ViewModels
{
    public class ProxyDispenserViewModel : ProxyDispenser, INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Dictionary<ProxyType, int> _StatsByType;
        public Dictionary<ProxyType, int> StatsByType
        {
            get => _StatsByType;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _StatsByType, value, PropertyChanged);
        }

        private Dictionary<ProxyState, int> _StatsByState;
        public Dictionary<ProxyState, int> StatsByState
        {
            get => _StatsByState;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _StatsByState, value, PropertyChanged);
        }

        private Dictionary<ProxyType, ArcViewModel> _ArcsByType;
        public Dictionary<ProxyType, ArcViewModel> ArcsByType
        {
            get => _ArcsByType;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcsByType, value, PropertyChanged);
        }

        private Dictionary<ProxyState, ArcViewModel> _ArcsByState;
        public Dictionary<ProxyState, ArcViewModel> ArcsByState
        {
            get => _ArcsByState;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcsByState, value, PropertyChanged);
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

        private ArcViewModel _ArcInvalid = new ArcViewModel(0, 1, Visibility.Hidden);
        public ArcViewModel ArcInvalid
        {
            get => _ArcInvalid;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcInvalid, value, PropertyChanged);
        }

        private ArcViewModel _ArcUnchecked = new ArcViewModel(0, 1, Visibility.Hidden);
        public ArcViewModel ArcUnchecked
        {
            get => _ArcUnchecked;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ArcUnchecked, value, PropertyChanged);
        }

        public void Refresh()
        {
            lock (Proxies)
            {
                foreach (ProxyType proxyType in Enum.GetValues(typeof(ProxyType)))
                    StatsByType[proxyType] = Proxies.Count(p => p.State == ProxyState.Valid
                    && p.Client.Type == proxyType);

                foreach (ProxyState proxyState in Enum.GetValues(typeof(ProxyState)))
                    StatsByState[proxyState] = Proxies.Count(p => p.State == proxyState);
            }

            var inst = this as INotifyPropertyChangedAdvanced;
            inst.OnPropertyChanged(PropertyChanged, nameof(StatsByType));

            int loaded = Math.Max(1, StatsByType.Values.Sum() + StatsByState[ProxyState.Invalid] + StatsByState[ProxyState.Unchecked]);
            Dictionary<ProxyType, float> sharesByType =
                StatsByType.ToDictionary(p => p.Key, p => (float)p.Value / loaded);
            Dictionary<ProxyState, float> sharesByState =
                StatsByState.ToDictionary(p => p.Key, p => (float)p.Value / loaded);

            float margin = 6;
            float pivot = margin / 2;
            int positiveShares =
                sharesByType.Values.Count(v => v > 0) +
                sharesByState.Count(s => s.Key != ProxyState.Valid && s.Value > 0);
            float maxPossibleAngle = 360 - (positiveShares * margin);
            foreach (var (proxyType, share) in sharesByType)
            {
                if (share == 0)
                {
                    ArcsByType[proxyType].StartAngle = 0;
                    ArcsByType[proxyType].EndAngle = 1;
                    ArcsByType[proxyType].Visibility = Visibility.Hidden;
                }
                else if (share == 1)
                {
                    ArcsByType[proxyType].StartAngle = 0;
                    ArcsByType[proxyType].EndAngle = 360;
                    ArcsByType[proxyType].Visibility = Visibility.Visible;
                }
                else
                {
                    ArcsByType[proxyType].StartAngle = pivot;
                    pivot += share * maxPossibleAngle;
                    ArcsByType[proxyType].EndAngle = pivot;
                    pivot += margin;
                    ArcsByType[proxyType].Visibility = Visibility.Visible;
                }
            }

            foreach (var (proxyState, share) in sharesByState)
            {
                if (proxyState == ProxyState.Valid)
                    continue;

                if (share == 0)
                {
                    ArcsByState[proxyState].StartAngle = 0;
                    ArcsByState[proxyState].EndAngle = 1;
                    ArcsByState[proxyState].Visibility = Visibility.Hidden;
                }
                else if (share == 1)
                {
                    ArcsByState[proxyState].StartAngle = 0;
                    ArcsByState[proxyState].EndAngle = 360;
                    ArcsByState[proxyState].Visibility = Visibility.Visible;
                }
                else
                {
                    ArcsByState[proxyState].StartAngle = pivot;
                    pivot += share * maxPossibleAngle;
                    ArcsByState[proxyState].EndAngle = pivot;
                    pivot += margin;
                    ArcsByState[proxyState].Visibility = Visibility.Visible;
                }
            }
        }

        public ProxyDispenserViewModel()
        {
            _StatsByType = new Dictionary<ProxyType, int>();
            foreach (ProxyType key in Enum.GetValues(typeof(ProxyType)))
                _StatsByType.Add(key, 0);

            _StatsByState = new Dictionary<ProxyState, int>();
            foreach (ProxyState key in Enum.GetValues(typeof(ProxyState)))
                _StatsByState.Add(key, 0);

            _ArcsByType = new Dictionary<ProxyType, ArcViewModel>();
            foreach (ProxyType key in Enum.GetValues(typeof(ProxyType)))
                _ArcsByType.Add(key, new ArcViewModel(0, 1, Visibility.Hidden));

            _ArcsByState = new Dictionary<ProxyState, ArcViewModel>();
            foreach (ProxyState key in Enum.GetValues(typeof(ProxyState)))
                _ArcsByState.Add(key, new ArcViewModel(0, 1, Visibility.Hidden));
        }
    }
}
