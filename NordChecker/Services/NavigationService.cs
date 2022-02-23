using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
    public class RegionFocusedEventArgs : EventArgs
    {
        public IRegion Region;

        public RegionFocusedEventArgs(IRegion region)
        {
            Region = region;
        }
    }

    public class NavigationService : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<RegionNavigationEventArgs> Navigating;
        public event EventHandler<RegionNavigationEventArgs> Navigated;
        public event EventHandler<RegionFocusedEventArgs> Focused;

        private readonly IRegionManager _RegionManager;

        public IRegion FocusedRegion { get; private set; }
        public object FocusedView { get; private set; }
        public bool IsOverlayFocused => FocusedRegion?.Name == "OverlayRegion";

        public NavigationService(IRegionManager regionManager)
        {
            _RegionManager = regionManager;

            Focused += (sender, e) =>
            {
                ((INotifyPropertyChangedAdvanced)this)
                .OnPropertyChanged(PropertyChanged, nameof(IsOverlayFocused));
            };
        }

        public void NavigateContent(string viewName) =>
            Navigate("ContentRegion", viewName);

        public void NavigateOverlay(string viewName) =>
            Navigate("OverlayRegion", viewName);

        public void Navigate(string regionName, string viewName)
        {
            var context = new NavigationContext(
                _RegionManager.Regions[regionName].NavigationService,
                new Uri(viewName, UriKind.Relative));
            var eventArgs = new RegionNavigationEventArgs(context);
            Navigating?.Invoke(this, eventArgs);
             
            var region = _RegionManager.Regions[regionName];

            bool toOverlay = !IsOverlayFocused && regionName == "OverlayRegion";
            if (!toOverlay && FocusedView != null)
            {
                Log.Error("Removing {0}", FocusedView.GetType());
                FocusedRegion.Remove(FocusedView);
                FocusedRegion.NavigationService.Journal.Clear();
                Log.Fatal("Last journal entry: {0}",
                    FocusedRegion.NavigationService.Journal.CurrentEntry?.Uri);
            }

            region.RequestNavigate(viewName);

            Focus(regionName);
            Log.Information("{0} navigated to {1}", regionName, viewName);

            Navigated?.Invoke(this, eventArgs);
        }

        public void Focus(string regionName)
        {
            if (FocusedRegion?.Name == regionName)
                return;

            FocusedRegion = _RegionManager.Regions[regionName];
            FocusedView = FocusedRegion.ActiveViews.First();
            Focused?.Invoke(this, new RegionFocusedEventArgs(FocusedRegion));
        }
    }
}
