using System;
using System.ComponentModel;
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
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Serilog;
using Unity;

namespace NordChecker.Services
{
//    public class NavigationContext : RegionNavigationEventArgs
//    {
//        public readonly string RegionName;
//        public readonly string ViewName;

//        public NavigationContext(string regionName, string viewName)
//        : base(new Prism.Regions.NavigationContext())
//        {
//            RegionName = regionName;
//            ViewName = viewName;
//        }
//    }

    public class NavigationService : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<RegionNavigationEventArgs> Navigating;
        public event EventHandler<RegionNavigationEventArgs> Navigated;
        public event EventHandler<NavigationContext> Focused;
        
        private string _FocusedRegion;
        public string FocusedRegion
        {
            get => _FocusedRegion;
            private set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _FocusedRegion, value, PropertyChanged);
        }

        private readonly IRegionManager _RegionManager;

        public NavigationService(IRegionManager regionManager)
        {
            _RegionManager = regionManager;
        }

        public void Navigate(string regionName, string viewName)
        {
            var eventArgs = new RegionNavigationEventArgs(
                new NavigationContext(
                    _RegionManager.Regions[regionName].NavigationService,
                    new Uri(viewName)));

            Navigating?.Invoke(this, eventArgs);
            _RegionManager.RequestNavigate(regionName, viewName);
            Focus(regionName);
            Navigated?.Invoke(this, eventArgs);
        }

        public void NavigateContent(string viewName) =>
            Navigate("ContentRegion", viewName);

        public void NavigateOverlay(string viewName) =>
            Navigate("OverlayRegion", viewName);

        public void GoBack()
        {
            var region = _RegionManager.Regions[FocusedRegion];
            var journal = region.NavigationService.Journal;

            if (journal.CanGoBack)
                journal.GoBack();
            else
                Focus("MainRegion");
        }

        public void Focus(string regionName) =>
            FocusedRegion = regionName;
    }
}
