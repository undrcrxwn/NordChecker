using NordChecker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using NordChecker.ViewModels;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Serilog;
using HandyControl.Themes;
using Newtonsoft.Json;
using NordChecker.Infrastructure;
using NordChecker.Models.Settings;

namespace NordChecker.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Wrapped<AppSettings> AppSettingsWrapped { get; set; }
        public MainWindowViewModel ViewModel { get; set; }

        private static void HideBoundingBox(object root)
        {
            if (root is Control control)
                control.FocusVisualStyle = null;

            if (root is DependencyObject @object)
            {
                foreach (object child in LogicalTreeHelper.GetChildren(@object))
                    HideBoundingBox(child);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(IntPtr ZeroOnly, string lpWindowName);

        public MainWindow(MainWindowViewModel viewModel, Wrapped<AppSettings> appSettingsWrapped)
        {
            AppSettingsWrapped = appSettingsWrapped;
            ViewModel = viewModel;
            DataContext = ViewModel;

            StateChanged += (sender, e) =>
            {
                if (AppSettingsWrapped.Instance.IsMinimizedToTray)
                {
                    ViewModel.WindowVisibility = WindowState != WindowState.Minimized
                        ? Visibility.Visible : Visibility.Collapsed;
                }
            };

            ViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.WindowVisibility))
                {
                    if (ViewModel.WindowVisibility == Visibility.Visible)
                    {
                        Log.Warning("ACTIVATE ATTEMPT    => {0}", Activate());
                        Log.Warning("FOCUS ATTEMPT       => {0}", Focus());
                    }
                }
            };

            InitializeComponent();

            NotifyIcon.Click += (sender, e) =>
                ViewModel.OpenFromTrayCommand.Execute(null);

            HideBoundingBox(this);
        }
    }
}
