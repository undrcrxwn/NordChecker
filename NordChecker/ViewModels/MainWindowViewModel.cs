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

        public NavigationService NavigationService { get; set; }
        public AppSettings AppSettings { get; set; }

        #region Properties

        private string _Title = "Загрузка...";
        public string Title
        {
            get => _Title;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Title, value, PropertyChanged);
        }

        private Visibility _WindowVisibility;
        public Visibility WindowVisibility
        {
            get => _WindowVisibility;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _WindowVisibility, value, PropertyChanged);
        }

        #endregion

        #region Commands

        #region OpenFromTrayCommand

        public ICommand OpenFromTrayCommand { get; }

        private bool CanExecuteOpenFromTrayCommand(object parameter) => true;

        private void OnOpenFromTrayCommandExecuted(object parameter)
        {
            Log.Information("OnOpenFromTrayCommandExecuted");
            WindowVisibility = Visibility.Visible;
        }

        #endregion

        #endregion

        private void UpdateTitle()
        {
            IPageViewModel pageViewModel = null;
            Application.Current.Dispatcher.Invoke(() =>
                pageViewModel = NavigationService.CurrentPage.DataContext as IPageViewModel);

            StringBuilder builder = new StringBuilder();
            builder.Append("NordVPN Checker");
            
            if (!string.IsNullOrEmpty(pageViewModel.Title))
                builder.Append($" — {pageViewModel.Title}");

            Title = builder.ToString();
        }

        private void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPageViewModel.Title))
                UpdateTitle();
        }

        private void UpdateAppearence()
        {
            ThemeManager.Current.ApplicationTheme = AppSettings.Theme;
            ThemeManager.Current.AccentColor = AppSettings.AccentColor;
        }

        public MainWindowViewModel(NavigationService navigationService, AppSettings appSettings)
        {
            NavigationService = navigationService;
            AppSettings = appSettings;

            NavigationService.Navigating += (sender, e) =>
            {
                if (NavigationService.CurrentPage == null) return;
                UpdateTitle();

                (NavigationService.CurrentPage.DataContext as IPageViewModel)
                .PropertyChanged -= OnPagePropertyChanged;
            };
            
            NavigationService.Navigated += (sender, e) =>
            {
                if (NavigationService.CurrentPage == null) return;
                UpdateTitle();

                (NavigationService.CurrentPage.DataContext as IPageViewModel)
                .PropertyChanged += OnPagePropertyChanged;
            };

            UpdateAppearence();
            AppSettings.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(AppSettings.AccentColor) ||
                    e.PropertyName == nameof(AppSettings.Theme))
                    UpdateAppearence();
            };

            OpenFromTrayCommand = new LambdaCommand(OnOpenFromTrayCommandExecuted, CanExecuteOpenFromTrayCommand);
        }
    }
}
