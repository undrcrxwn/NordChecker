using HandyControl.Themes;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
using NordChecker.Commands;
using NordChecker.Models;
using NordChecker.Shared;
using NordChecker.Views;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using Leaf.xNet;
using System.Windows.Media.Effects;
using HandyControl.Tools.Command;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace NordChecker.ViewModels
{
    public enum PipelineState
    {
        Idle,
        Paused,
        Working
    }

    public class MainWindowViewModel : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private INavigationService navigationService;

        public AppSettings AppSettings { get; set; }

        private string _Title = "Загрузка...";
        public string Title
        {
            get => _Title;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Title, value, PropertyChanged);
        }

        private void UpdateTitle()
        {
            IPageViewModel currentPage = navigationService.CurrentPage;
            Title = "NordVPN Checker";
            if (!string.IsNullOrEmpty(currentPage.Title))
                Title += $" — {currentPage.Title}";
            if (!string.IsNullOrEmpty(currentPage.Description))
                Title += $" — {currentPage.Description}";
        }

        public MainWindowViewModel(INavigationService navigationService, AppSettings appSettings, ExportSettings exportSettings)
        {
            this.navigationService = navigationService;
            navigationService.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(navigationService.CurrentPage))
                    UpdateTitle();
            };
            
            AppSettings = appSettings;
        }
    }
}
