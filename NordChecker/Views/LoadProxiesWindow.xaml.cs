using NordChecker.Models;
using NordChecker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NordChecker.Views
{
    /// <summary>
    /// Interaction logic for LoadProxiesWindow.xaml
    /// </summary>
    public partial class LoadProxiesWindow : Window
    {
        public LoadProxiesWindow()
        {
            InitializeComponent();
            DataContext = new LoadProxiesWindowViewModel();
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e)
        {
            ((LoadProxiesWindowViewModel)DataContext).IsOperationConfirmed = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
