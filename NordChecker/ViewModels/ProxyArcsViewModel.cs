using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Tools.Extension;
using Leaf.xNet;
using NordChecker.Infrastructure;
using NordChecker.Models;
using NordChecker.Models.Stats;
using NordChecker.Shared.Collections;

namespace NordChecker.ViewModels
{
    public class ProxyArcsViewModel : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ProxyStats ProxyStats;
        
        private Dictionary<ProxyType, Arc> _ValidByType;
        public Dictionary<ProxyType, Arc> ValidByType
        {
            get => _ValidByType;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ValidByType, value, PropertyChanged);
        }

        private Arc _InvalidArc;
        public Arc InvalidArc
        {
            get => _InvalidArc;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _InvalidArc, value, PropertyChanged);
        }
        
        public ProxyArcsViewModel(ProxyStats proxyStats)
        {
            ProxyStats = proxyStats;

            ValidByType = Enum.GetValues<ProxyType>().Reverse()
                .ToDictionary(x => x, _ => new Arc(0, 1, Visibility.Hidden));
        }

        public void Refresh()
        {
            int loaded = Math.Max(1, ProxyStats.ValidByType.Values.Sum() + ProxyStats.InvalidCount);
            Dictionary<ProxyType, float> sharesByType =
                ProxyStats.ValidByType.ToDictionary(x => x.Key, x => (float)x.Value / loaded);

            const float margin = 6;
            float pivot = margin / 2;
            int positiveShares =
                sharesByType.Values.Count(x => x > 0) + (ProxyStats.InvalidCount > 0 ? 1 : 0);
            float maxPossibleAngle = 360 - positiveShares * margin;
            foreach (var (proxyType, share) in sharesByType)
            {
                switch (share)
                {
                    case 0:
                        ValidByType[proxyType].StartAngle = 0;
                        ValidByType[proxyType].EndAngle = 1;
                        ValidByType[proxyType].Visibility = Visibility.Hidden;
                        break;
                    case 1:
                        ValidByType[proxyType].StartAngle = 0;
                        ValidByType[proxyType].EndAngle = 360;
                        ValidByType[proxyType].Visibility = Visibility.Visible;
                        break;
                    default:
                        ValidByType[proxyType].StartAngle = pivot;
                        pivot += share * maxPossibleAngle;
                        ValidByType[proxyType].EndAngle = pivot;
                        pivot += margin;
                        ValidByType[proxyType].Visibility = Visibility.Visible;
                        break;
                }
            }
            
            float invalidShare = (float)ProxyStats.InvalidCount / loaded;
            switch (invalidShare)
            {
                case 0:
                    InvalidArc.StartAngle = 0;
                    InvalidArc.EndAngle = 1;
                    InvalidArc.Visibility = Visibility.Hidden;
                    break;
                case 1:
                    InvalidArc.StartAngle = 0;
                    InvalidArc.EndAngle = 360;
                    InvalidArc.Visibility = Visibility.Visible;
                    break;
                default:
                    InvalidArc.StartAngle = pivot;
                    pivot += invalidShare * maxPossibleAngle;
                    InvalidArc.EndAngle = pivot;
                    InvalidArc.Visibility = Visibility.Visible;
                    break;
            }
            
            (this as INotifyPropertyChangedAdvanced)
                .OnPropertyChanged(PropertyChanged, nameof(ValidByType));
        }
    }
}
