using Microsoft.Win32;
using Serilog;
using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;

namespace NordChecker.Shared
{
    public static class WindowHelper
    {
        [DllImport("kernel32.dll")]
        public static extern void AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern void FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        public static void ShowConsole() =>
            ShowWindow(GetConsoleWindow(), SW_SHOW);

        public static void HideConsole() =>
            ShowWindow(GetConsoleWindow(), SW_HIDE);
    }

    public static class RegexHelper
    {
        public static string[] Match(string input, string regexPattern)
        {
            Match match = Regex.Match(input, regexPattern);
            if (match.Success)
                return match.Groups.Values.Select(x => x.Value).ToArray();
            throw new InvalidOperationException();
        }
    }

    public static class BindingHelper
    {
        public static PropertyChangedEventHandler BindOneWay(
            object sourceObject, string sourcePropertyName,
            object dependentObject, string dependentPropertyName)
        {
            var sourceProperty = sourceObject.GetType().GetProperty(sourcePropertyName);
            var dependentProperty = dependentObject.GetType().GetProperty(dependentPropertyName);

            if (sourceObject is INotifyPropertyChanged notifier)
            {
                PropertyChangedEventHandler OnNotifierPropertyChanged = (sender, e) =>
                {
                    if (e.PropertyName == sourcePropertyName)
                    {
                        SynchronizeProperties(
                            sourceObject, sourcePropertyName,
                            dependentObject, dependentPropertyName);
                    }
                };

                SynchronizeProperties(
                    sourceObject, sourcePropertyName,
                    dependentObject, dependentPropertyName);
                notifier.PropertyChanged += OnNotifierPropertyChanged;
                
                return OnNotifierPropertyChanged;
            }
            else
                throw new ArgumentException("The source object is required to implement the INotifyPropertyChanged interface.", nameof(sourceObject));
        }

        public static void UnbindOneWay(
            object sourceObject,
            PropertyChangedEventHandler handler)
        {
            if (sourceObject is INotifyPropertyChanged notifier)
                notifier.PropertyChanged -= handler;
            else
                throw new ArgumentException("The source object is required to implement the INotifyPropertyChanged interface.", nameof(sourceObject));
        }

        private static void SynchronizeProperties(
            object sourceObject, string sourcePropertyName,
            object dependentObject, string dependentPropertyName)
        {
            try
            {
                PropertyInfo sourcePropertyInfo = sourceObject.GetType().GetProperty(sourcePropertyName);
                PropertyInfo dependentPropertyInfo = dependentObject.GetType().GetProperty(dependentPropertyName);

                object sourcePropertyValue = sourcePropertyInfo.GetValue(sourceObject, null);
                object dependentPropertyValue = dependentPropertyInfo.GetValue(dependentObject, null);

                if (sourcePropertyValue == dependentPropertyValue) return;

                dependentPropertyInfo.SetValue(dependentObject, sourcePropertyValue);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "An exception was thrown while synchronizing property values within a one-way binding.");
            }
        }
    }
}
