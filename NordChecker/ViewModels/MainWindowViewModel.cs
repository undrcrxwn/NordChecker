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

        public MainWindowViewModel(
            IRegionManager regionManager,
            NavigationService navigationService,
            AppSettings appSettings)
        {
            NavigationService = navigationService;
            AppSettings = appSettings;

            NavigationService.Navigating += (sender, e) =>
            {
                if (NavigationService.FocusedRegion == null)
                    return;

                if (NavigationService.FocusedView is Page page)
                    UnbindPageTitle((IPageViewModel)page.DataContext);
            };

            NavigationService.Navigated += (sender, e) =>
            {
                if (NavigationService.FocusedView is Page page)
                    BindPageTitle((IPageViewModel)page.DataContext);
            };

            AppSettings.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName
                    is nameof(AppSettings.AccentColor)
                    or nameof(AppSettings.Theme))
                    RefreshAppearence();
            };
            RefreshAppearence();

            OpenFromTrayCommand = new DelegateCommand(OnOpenFromTrayCommandExecuted);
        }

        private void RefreshAppearence()
        {
            ThemeManager.Current.ApplicationTheme = AppSettings.Theme;
            ThemeManager.Current.AccentColor = AppSettings.AccentColor;
            Log.Warning("Updated to {0}", AppSettings.AccentColor);
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
