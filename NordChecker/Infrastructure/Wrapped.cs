using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NordChecker.Models.Settings;

namespace NordChecker.Infrastructure
{
    public class Wrapped<T> : INotifyPropertyChangedAdvanced where T : class
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private T _Instance;
        public T Instance
        {
            get => _Instance;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Instance, value, PropertyChanged);
        }

        public Wrapped(T instance) => _Instance = instance;

        public void ReplaceWith(T instance) => Instance = instance;
    }
}
