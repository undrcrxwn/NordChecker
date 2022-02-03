using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using NordChecker.Infrastructure;
using NordChecker.Models.Stats;
using NordChecker.ViewModels;
using NordChecker.Views;
using Prism.Common;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Serilog;
using Unity;

namespace NordChecker.Services
{
    public record RegionFocusedEventArgs(IRegion Region);

    public class NavigationService : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<RegionNavigationEventArgs> Navigating;
        public event EventHandler<RegionNavigationEventArgs> Navigated;
        public event EventHandler<RegionFocusedEventArgs> Focused;

        private readonly IRegionManager _RegionManager;

        public IRegion FocusedRegion { get; private set; }
        public bool IsOverlayFocused => FocusedRegion?.Name == "OverlayRegion";

        public NavigationService(IRegionManager regionManager)
        {
            _RegionManager = regionManager;

            Focused += (sender, e) =>
                (this as INotifyPropertyChangedAdvanced)
                .OnPropertyChanged(PropertyChanged, nameof(IsOverlayFocused));
        }

        public void Navigate(string regionName, string viewName)
        {
            var eventArgs = new RegionNavigationEventArgs(
                new NavigationContext(
                    _RegionManager.Regions[regionName].NavigationService,
                    new Uri(viewName, UriKind.Relative)));

            Navigating?.Invoke(this, eventArgs);
            _RegionManager.RequestNavigate(regionName, viewName);
            Focus(regionName);
            Navigated?.Invoke(this, eventArgs);
        }

        public void NavigateContent(string viewName) =>
            Navigate("ContentRegion", viewName);

        public void NavigateOverlay(string viewName) =>
            Navigate("OverlayRegion", viewName);

        public void Focus(string regionName)
        {
            if (FocusedRegion?.Name == regionName) return;
            FocusedRegion = _RegionManager.Regions[regionName];
            Focused?.Invoke(this, new RegionFocusedEventArgs(FocusedRegion));
        }
    }
}
