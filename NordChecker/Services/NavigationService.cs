using System;
using System.ComponentModel;
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
using Prism.Unity;
using Serilog;
using Unity;

namespace NordChecker.Services
{
    public class NavigationService : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<Page> Navigating;
        public event EventHandler<Page> Navigated;
        private readonly IUnityContainer _Container;
        
        private Page _ContentPage;
        public Page ContentPage
        {
            get => _ContentPage;
            private set
            {
                Navigating?.Invoke(this, ContentPage);
                (this as INotifyPropertyChangedAdvanced)
                    .Set(ref _ContentPage, value, PropertyChanged);
                Navigated?.Invoke(this, ContentPage);
            }
        }

        public NavigationService(IUnityContainer container)
        {
            _Container = container;
        }
        
        public void Navigate(Page page) => ContentPage = page;

        public void Navigate<TPage>() where TPage : Page =>
            Navigate(_Container.Resolve<TPage>());
    }
}
