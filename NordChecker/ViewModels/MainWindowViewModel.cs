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
using System.Windows.Controls;
using NordChecker.Models.Settings;
using NordChecker.Infrastructure;
using NordChecker.Services;
using Prism.Commands;
using Unity;
using Prism.Regions;

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
        
        public MainWindowViewModel(IRegionManager regionManager, NavigationService navigationService, Wrapped<AppSettings> appSettingsWrapped)
        {
            NavigationService = navigationService;
            AppSettingsWrapped = appSettingsWrapped;
            
            NavigationService.Navigating += (sender, e) =>
            {
                if (NavigationService.FocusedRegion == null) return;
                object view = NavigationService.FocusedRegion.ActiveViews.First();
                if (view is Page page)
                    UnbindPageTitle((IPageViewModel)page.DataContext);
            };

            NavigationService.Navigated += (sender, e) =>
            {
                if (NavigationService.FocusedRegion == null) return;
                //if (!NavigationService.FocusedRegion.ActiveViews.Any()) return;

                object view = NavigationService.FocusedRegion.Views.First();

                //object view = new Page(e.NavigationContext.Uri, UriKind.Relative);
                if (view is Page page)
                    BindPageTitle((IPageViewModel)page.DataContext);
            };

            AppSettingsWrapped.ForEach(appSettings =>
            {
                appSettings.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName
                        is nameof(AppSettingsWrapped.Instance.AccentColor)
                        or nameof(AppSettingsWrapped.Instance.Theme))
                        RefreshAppearence();
                };
                RefreshAppearence();
            });

            OpenFromTrayCommand = new DelegateCommand(OnOpenFromTrayCommandExecuted);
        }

        private void RefreshAppearence()
        {
            ThemeManager.Current.ApplicationTheme = AppSettingsWrapped.Instance.Theme;
            ThemeManager.Current.AccentColor = AppSettingsWrapped.Instance.AccentColor;
            Log.Warning("Updated to {0}", AppSettingsWrapped.Instance.AccentColor);
        }

        private void BindPageTitle(IPageViewModel viewModel)
        {
            InheritTitle(viewModel);
            viewModel.PropertyChanged += OnPageViewModelPropertyChanged;
        }

        private void UnbindPageTitle(IPageViewModel viewModel)
            => viewModel.PropertyChanged -= OnPageViewModelPropertyChanged;

        private void OnPageViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPageViewModel.Title))
                InheritTitle((IPageViewModel)sender);
        }

        private void InheritTitle(IPageViewModel pageViewModel)
        {
            var builder = new StringBuilder();
            builder.Append("NordVPN Checker");

            if (!string.IsNullOrEmpty(pageViewModel.Title))
                builder.Append($" — {pageViewModel.Title}");

            Title = builder.ToString();
        }
    }
}
