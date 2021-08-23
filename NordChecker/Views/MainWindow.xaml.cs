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

namespace NordChecker.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm = null;

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
            if (vm == null) return;
            (
                vm.VisibilityFilters[AccountState.Unchecked],
                vm.VisibilityFilters[AccountState.Reserved],
                vm.VisibilityFilters[AccountState.Invalid],
                vm.VisibilityFilters[AccountState.Free],
                vm.VisibilityFilters[AccountState.Premium]
            ) = (
                btnAreUncheckedDisplayed.IsChecked ?? false,
                btnAreReservedDisplayed.IsChecked ?? false,
                btnAreInvalidDisplayed.IsChecked ?? false,
                btnAreFreeDisplayed.IsChecked ?? false,
                btnArePremiumDisplayed.IsChecked ?? false
            );
            Log.Information("DataGrid filters have been updated: {filters}", vm.VisibilityFilters);

            ICollectionView cv = dgAccounts.ItemsSource as ICollectionView;
            Dispatcher.Invoke(() =>
            {
                cv.Filter = (acc) => vm.VisibilityFilters[(acc as Account).State];
                cv.Refresh();
            });
            Log.Information("New DataGrid filters have been applied");
        }

        private void OnFilteringSettingsUpdated(object sender, RoutedEventArgs e) => UpdateFiltering();

        public MainWindow()
        {
            InitializeComponent();
            HideBoundingBox(this);

            dgAccounts.SelectionChanged += (obj, e) =>
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                    dgAccounts.UnselectAll()));

            vm = (MainWindowViewModel)DataContext;
            var _itemSourceList = new CollectionViewSource() { Source = vm.ComboBase.Accounts };
            ICollectionView cv = _itemSourceList.View;
            dgAccounts.ItemsSource = cv;
            dgAccounts.Items.IsLiveSorting = true;

            new Thread(() =>
            {
                while (true)
                {
                    var filter = cv.Filter;
                    Dispatcher.BeginInvoke(() => cv.Filter = filter);

                    //dgAccounts.ItemsSource = cv;

                    Thread.Sleep(500);
                }
            }){ IsBackground = true }.Start();
        }
    }
}