using NordChecker.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    internal class ComboBaseStatistics_TO_BE_REMOVED : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _Loaded;
        public int Loaded
        {
            get => _Loaded;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Loaded, value, PropertyChanged);
        }

        private int _Premium;
        public int Premium
        {
            get => _Premium;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Premium, value, PropertyChanged);
        }

        private int _Free;
        public int Free
        {
            get => _Free;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Free, value, PropertyChanged);
        }

        private int _Invalid;
        public int Invalid
        {
            get => _Invalid;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Invalid, value, PropertyChanged);
        }

        private int _Reserved;
        public int Reserved
        {
            get => _Reserved;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Reserved, value, PropertyChanged);
        }

        private int _Unchecked;
        public int Unchecked
        {
            get => _Unchecked;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Unchecked, value, PropertyChanged);
        }

        private int _Mismatched;
        public int Mismatched
        {
            get => _Mismatched;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Mismatched, value, PropertyChanged);
        }

        private int _Duplicates;
        public int Duplicates
        {
            get => _Duplicates;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Duplicates, value, PropertyChanged);
        }
    }
}
