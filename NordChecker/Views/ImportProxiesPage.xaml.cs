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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NordChecker.ViewModels;

namespace NordChecker.Views
{
    /// <summary>
    /// Логика взаимодействия для LoadProxiesPage.xaml
    /// </summary>
    public partial class ImportProxiesPage : Page
    {
        public ImportProxiesPage(ImportProxiesPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
