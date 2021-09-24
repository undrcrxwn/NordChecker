using NordChecker.Models;
using NordChecker.ViewModels;
using Serilog;
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
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        public ExportWindow(ExportSettings settings)
        {
            InitializeComponent();
            DataContext = new ExportWindowViewModel(settings);
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as ExportWindowViewModel).IsOperationConfirmed = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
