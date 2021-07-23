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
            ICollectionView cv = dgAccounts.ItemsSource as ICollectionView;
            if (cv == null) return;

            Dispatcher.Invoke(() =>
            {
                cv.Filter = (acc) =>
                {
                    return (acc as Account).State switch
                    {
                        AccountState.Unchecked => vm.AreUncheckedDisplayed,
                        AccountState.Invalid => vm.AreInvalidDisplayed,
                        AccountState.Free => vm.AreFreeDisplayed,
                        AccountState.Premium => vm.ArePremiumDisplayed,
                        _ => false
                    };
                };
                cv.Refresh();
            });
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
            var _itemSourceList = new CollectionViewSource() { Source = vm.CurrentBase.Accounts };
            ICollectionView cv = _itemSourceList.View;
            dgAccounts.ItemsSource = cv;
            dgAccounts.Items.IsLiveSorting = true;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timer.Tick += (object sender, EventArgs e) => UpdateFiltering();
            timer.Start();
        }
    }
}