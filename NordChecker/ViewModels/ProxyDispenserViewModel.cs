using Leaf.xNet;
using NordChecker.Models;
using NordChecker.Shared;
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

        private Dictionary<ProxyType, int> _Stats;
        public Dictionary<ProxyType, int> Stats
        {
            get => _Stats;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Stats, value, PropertyChanged);
        }

        private Dictionary<ProxyType, ArcViewModel> _Arcs;
        public Dictionary<ProxyType, ArcViewModel> Arcs
        {
            get => _Arcs;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Arcs, value, PropertyChanged);
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
                    Stats[proxyType] = Proxies.Count(p => p.State == ProxyState.Valid
                    && p.Client.Type == proxyType);
            }

            var inst = this as INotifyPropertyChangedAdvanced;
            inst.OnPropertyChanged(PropertyChanged, Utils.GetMemberName(() => Stats));

            int loaded = Math.Max(1, Stats.Values.Sum());
            Dictionary<ProxyType, float> shares =
                Stats.ToDictionary(p => p.Key, p => (float)p.Value / loaded);

            float margin = 6;
            float pivot = margin / 2;
            float maxPossibleAngle = 360 - (shares.Values.Count(v => v > 0) * margin);
            foreach (var (state, share) in shares)
            {
                if (share == 0)
                {
                    Arcs[state].StartAngle = 0;
                    Arcs[state].EndAngle = 0;
                    Arcs[state].Visibility = Visibility.Hidden;
                }
                else if (share == 1)
                {
                    Arcs[state].StartAngle = 0;
                    Arcs[state].EndAngle = 360;
                    Arcs[state].Visibility = Visibility.Visible;
                }
                else
                {
                    Arcs[state].StartAngle = pivot;
                    pivot += share * maxPossibleAngle;
                    Arcs[state].EndAngle = pivot;
                    pivot += margin;
                    Arcs[state].Visibility = Visibility.Visible;
                }
            }
        }

        public ProxyDispenserViewModel()
        {
            _Stats = new Dictionary<ProxyType, int>();
            foreach (ProxyType key in Enum.GetValues(typeof(ProxyType)))
                _Stats.Add(key, 0);

            _Arcs = new Dictionary<ProxyType, ArcViewModel>();
            foreach (ProxyType key in Enum.GetValues(typeof(ProxyType)))
                _Arcs.Add(key, new ArcViewModel(0, 1, Visibility.Hidden));
        }
    }
}
