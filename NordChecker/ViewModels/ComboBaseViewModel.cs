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

        private Dictionary<AccountState, int> _Stats;
        public Dictionary<AccountState, int> Stats
        {
            get => _Stats;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Stats, value, PropertyChanged);
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

        private Dictionary<AccountState, ArcViewModel> _Arcs;
        public Dictionary<AccountState, ArcViewModel> Arcs
        {
            get => _Arcs;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Arcs, value, PropertyChanged);
        }

        public void Refresh()
        {
            foreach (AccountState accountState in Enum.GetValues(typeof(AccountState)))
                Stats[accountState] = Accounts.Count(a => a.State == accountState);
            var inst = this as INotifyPropertyChangedAdvanced;
            inst.OnPropertyChanged(PropertyChanged, nameof(Stats));

            int loaded = Math.Max(1, Stats.Values.Sum());
            Dictionary<AccountState, float> shares =
                Stats.ToDictionary(p => p.Key, p => (float)p.Value / loaded);

            float margin = 6;
            float pivot = margin / 2;
            float maxPossibleAngle = 360 - (shares.Values.Count(v => v > 0) * margin);
            foreach (var (state, share) in shares)
            {
                if (share == 0)
                {
                    Arcs[state].StartAngle = 0;
                    Arcs[state].EndAngle = 1;
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

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Application.Current.MainWindow.TaskbarItemInfo ??= new System.Windows.Shell.TaskbarItemInfo();
                if (Stats[AccountState.Unchecked] + Stats[AccountState.Reserved] > 0)
                {
                    Application.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                    Application.Current.MainWindow.TaskbarItemInfo.ProgressValue
                        = 1 - shares[AccountState.Unchecked] - shares[AccountState.Reserved];
                }
                else
                {
                    Application.Current.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    Application.Current.MainWindow.TaskbarItemInfo.ProgressValue = 0;
                }
            });
        }

        public ComboBaseViewModel()
        {
            _Stats = new Dictionary<AccountState, int>();
            foreach (AccountState key in Enum.GetValues(typeof(AccountState)))
                _Stats.Add(key, 0);

            _Arcs = new Dictionary<AccountState, ArcViewModel>();
            foreach (AccountState key in Enum.GetValues(typeof(AccountState)))
                _Arcs.Add(key, new ArcViewModel(0, 1, Visibility.Hidden));
        }
    }
}
