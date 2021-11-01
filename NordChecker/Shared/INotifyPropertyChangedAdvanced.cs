using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Shared
{
    public interface INotifyPropertyChangedAdvanced : INotifyPropertyChanged
    {
        public virtual void OnPropertyChanged(PropertyChangedEventHandler handler, [CallerMemberName] string propertyName = null)
        {
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual bool Set<T>(
            ref T field, T value,
            PropertyChangedEventHandler handler,
            LogEventLevel logEventLevel = LogEventLevel.Verbose,
            [CallerMemberName] string propertyName = null)
        {
            if (field != null && value != null && field.Equals(value))
                return false;
            field = value;

            MethodBase methodInfo = new StackTrace().GetFrame(1).GetMethod();
            string className = methodInfo.ReflectedType.Name;

            LogPropertyChanged(logEventLevel, propertyName, value, className);
            OnPropertyChanged(handler, propertyName);
            return true;
        }

        public void LogPropertyChanged<T>(LogEventLevel logEventLevel, string propertyName, T value, string className = null) =>
            Log.Write(logEventLevel, "{0} property of {1} has been set to {2}", propertyName, className, value);
    }
}
