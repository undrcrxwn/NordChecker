using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Shared
{
    internal interface INotifyPropertyChangedAdvanced : INotifyPropertyChanged
    {
        public virtual void OnPropertyChanged(PropertyChangedEventHandler handler, [CallerMemberName] string propertyName = null)
        {
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual bool Set<T>(ref T field, T value, PropertyChangedEventHandler handler, LogEventLevel logEventLevel = LogEventLevel.Verbose, [CallerMemberName] string propertyName = null)
        {
            if (field != null && value != null && field.Equals(value))
                return false;
            field = value;
            Log.Write(logEventLevel, "{caller} has been set to {state}", propertyName, value);
            OnPropertyChanged(handler, propertyName);
            return true;
        }
    }
}
