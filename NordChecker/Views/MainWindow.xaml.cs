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

namespace NordChecker.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IAppSettings Settings { get; set; }

        private static void HideBoundingBox(object root)
        {
            Control control = root as Control;
            if (control != null)
                control.FocusVisualStyle = null;

            if (root is DependencyObject)
            {
                foreach (object child in LogicalTreeHelper.GetChildren((DependencyObject)root))
                    HideBoundingBox(child);
            }
        }

        private void UpdateFiltering()
        {
            ICollectionView cv = dgAccounts.ItemsSource as ICollectionView;
            Dispatcher.Invoke(() =>
            {
                cv.Filter = (acc) => Settings.DataGridFilters[(acc as Account).State];
                cv.Refresh();
            });
            Log.Information("New DataGrid filters have been applied");
        }

        private void OnFilteringSettingsUpdated(object sender, RoutedEventArgs e) => UpdateFiltering();

        public MainWindow() { }

        public MainWindow(MainWindowViewModel viewModel, IAppSettings settings)
        {
            InitializeComponent();
            HideBoundingBox(this);

            Settings = settings;

            settings.DataGridFilters.CollectionChanged +=
                (object sender, NotifyCollectionChangedEventArgs e) =>
                UpdateFiltering();

            var source = new CollectionViewSource() { Source = viewModel.ComboBase.Accounts };
            ICollectionView cv = source.View;
            dgAccounts.ItemsSource = cv;

            new Thread(() =>
            {
                while (true)
                {
                    while (viewModel.SelectedAccount != null)
                        Thread.Sleep(10);
                    var filter = cv.Filter;
                    Dispatcher.BeginInvoke(() => cv.Refresh()).Wait();
                    Thread.Sleep(500);
                }
            })
            { IsBackground = true }.Start();
        }

        private void ColorPicker_SelectedColorChanged(object sender, HandyControl.Data.FunctionEventArgs<Color> e)
        {
            Settings.AccentColor = (sender as HandyControl.Controls.ColorPicker).SelectedBrush;
        }
    }
}
