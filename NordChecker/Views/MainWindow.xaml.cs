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
    public partial class MainWindow : NavigationWindow
    {
        public AppSettings Settings { get; set; }

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

        public MainWindow() { }

        public MainWindow(AppSettings settings)
        {
            InitializeComponent();
            HideBoundingBox(this);
        }
    }
}
