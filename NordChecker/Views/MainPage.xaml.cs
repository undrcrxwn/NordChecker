using NordChecker.Models;
using NordChecker.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
using NordChecker.Models.Settings;
using NordChecker.Models.Domain;

namespace NordChecker.Views
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public AppSettings AppSettings { get; set; }

        public MainPage(MainPageViewModel viewModel)
        {
            DataContext = viewModel;
            AppSettings = viewModel.AppSettings;
            InitializeComponent();

            AppSettings.DataGridFilters.CollectionChanged +=
                (object sender, NotifyCollectionChangedEventArgs e) =>
                UpdateFiltering();

            var source = new CollectionViewSource() { Source = viewModel.Accounts };
            ICollectionView cv = source.View;
            dgAccounts.ItemsSource = cv;

            Task.Run(() =>
            {
                while (true)
                {
                    while (viewModel.SelectedAccount != null)
                        Thread.Sleep(10);
                    Application.Current.Dispatcher.BeginInvoke(cv.Refresh);
                    Thread.Sleep(500);
                }
            });
        }

        private void UpdateFiltering()
        {
            ICollectionView cv = dgAccounts.ItemsSource as ICollectionView;
            Dispatcher.Invoke(() =>
            {
                cv.Filter = (acc) => AppSettings.DataGridFilters[(acc as Account).State];
                cv.Refresh();
            });
            Log.Information("New DataGrid filters have been applied");
        }

        private void OnFilteringSettingsUpdated(object sender, RoutedEventArgs e) => UpdateFiltering();

        private void ColorPicker_SelectedColorChanged(object sender, HandyControl.Data.FunctionEventArgs<Color> e)
        {
            AppSettings.AccentColor = (sender as HandyControl.Controls.ColorPicker).SelectedBrush;
        }
    }
}
