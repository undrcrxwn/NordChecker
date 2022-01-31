using HandyControl.Themes;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
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
using System.Windows.Threading;
using System.Threading.Tasks;
using NordChecker.Models.Settings;
using NordChecker.Infrastructure;
using NordChecker.Services;
using Prism.Commands;

namespace NordChecker.ViewModels
{
    public enum PipelineState
    {
        Idle,
        Paused,
        Working
    }

    public partial class MainWindowViewModel : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NavigationService NavigationService { get; set; }
        public Wrapped<AppSettings> AppSettingsWrapped { get; set; }

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
        
        private void UpdateTitle()
        {
            IPageViewModel pageViewModel = null;
            //Application.Current.Dispatcher.Invoke(() =>
            //    pageViewModel = (IPageViewModel)NavigationService.ContentPage.DataContext);

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
            ThemeManager.Current.ApplicationTheme = AppSettingsWrapped.Instance.Theme;
            ThemeManager.Current.AccentColor = AppSettingsWrapped.Instance.AccentColor;
            Log.Warning("Updated to {0}", AppSettingsWrapped.Instance.AccentColor);
        }

        public MainWindowViewModel(NavigationService navigationService, Wrapped<AppSettings> appSettingsWrapped)
        {
            NavigationService = navigationService;
            AppSettingsWrapped = appSettingsWrapped;

            NavigationService.Navigating += (sender, e) =>
            {
                //if (NavigationService.ContentPage == null) return;
                //UpdateTitle();

                //((IPageViewModel)NavigationService.ContentPage.DataContext)
                //    .PropertyChanged -= OnPagePropertyChanged;
            };
            
            NavigationService.Navigated += (sender, e) =>
            {
                //if (NavigationService.ContentPage == null) return;
                //UpdateTitle();

                //((IPageViewModel)NavigationService.ContentPage.DataContext)
                //    .PropertyChanged += OnPagePropertyChanged;
            };
            
            AppSettingsWrapped.ForEach(appSettings =>
            {
                appSettings.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName
                        is nameof(AppSettingsWrapped.Instance.AccentColor)
                        or nameof(AppSettingsWrapped.Instance.Theme))
                        UpdateAppearence();
                };
                UpdateAppearence();
            });

            OpenFromTrayCommand = new DelegateCommand(OnOpenFromTrayCommandExecuted);
        }
    }
}
