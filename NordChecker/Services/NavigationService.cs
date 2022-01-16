using System;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NordChecker.Infrastructure;
using Prism.Unity;

namespace NordChecker.Services
{
    public class NavigationService : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<Page> Navigating;
        public event EventHandler<Page> Navigated;

        private Page _CurrentPage;
        public Page CurrentPage
        {
            get => _CurrentPage;
            private set
            {
                Navigating?.Invoke(this, CurrentPage);
                (this as INotifyPropertyChangedAdvanced)
                    .Set(ref _CurrentPage, value, PropertyChanged);
                Navigated?.Invoke(this, CurrentPage);
            }
        }

        public void Navigate(Page page) => CurrentPage = page;

        public void Navigate<TPage>() where TPage : Page
        {
            CurrentPage = (TPage)Prism.Ioc.ContainerLocator.Current.Resolve(typeof(TPage));
            //CurrentPage = PrismApplication.Current..ServiceProvider.GetService<TPage>();
            Navigate(CurrentPage);
        }
    }
}
