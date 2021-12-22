﻿using NordChecker.Models;
using NordChecker.Shared;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Serilog;

namespace NordChecker.Views
{
    /// <summary>
    /// Interaction logic for ExportPage.xaml
    /// </summary>
    public partial class ExportPage : Page
    {
        public ExportPage(ExportPageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Log.Warning("Page {0} c-tor: settings hash = {1}", this.GetHashCode(), ((ExportPageViewModel)DataContext).ExportSettings.GetHashCode());

            var es = ((ExportPageViewModel)DataContext).ExportSettings;
            Log.Warning("AFTER VM TAKEN FOR CONTEXT public ExportPage(ExportPageViewModel viewModel)");
            Log.Warning("SETTINGS = {0}, FILTERS = {1}, PREMIUM = {2}",
                es.GetHashCode(), es.Filters.GetHashCode(), es.Filters.Premium.GetHashCode());
        }
    }
}
