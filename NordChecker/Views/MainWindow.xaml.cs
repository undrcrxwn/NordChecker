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

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            Task t = e.Item as Task;
            if (t != null)
            // If filter is turned on, filter completed items.
            {
                if (this.cbCompleteFilter.IsChecked == true && t.Complete == true)
                    e.Accepted = false;
                else
                    e.Accepted = true;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            HideBoundingBox(this);

            dgAccounts.SelectionChanged += (obj, e) =>
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                    dgAccounts.UnselectAll()));

            vm = (MainWindowViewModel)DataContext;
            dgAccounts.ItemsSource = vm.CurrentBase.Accounts;
            dgAccounts.Items.IsLiveSorting = true;
        }
    }
}
