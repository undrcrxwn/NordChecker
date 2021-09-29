using Microsoft.Extensions.DependencyInjection;
using NordChecker.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace NordChecker.Shared
{
    public interface INavigationService : INotifyPropertyChangedAdvanced
    {
        public IPageViewModel CurrentPage { get; }
        public void Navigate(Page page);
        public void Navigate<TPage>() where TPage : Page;
    }

    public sealed class NavigationService : INavigationService
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private PropertyChangedEventHandler CurrentPagePropertyChangedHandler => (sender, e) =>
            (this as INotifyPropertyChangedAdvanced).OnPropertyChanged(PropertyChanged, nameof(CurrentPage));

        private IPageViewModel _CurrentPage;
        public IPageViewModel CurrentPage
        {
            get => _CurrentPage;
            private set
            {
                if (CurrentPage != null)
                    CurrentPage.PropertyChanged -= CurrentPagePropertyChangedHandler;
                INotifyPropertyChangedAdvanced @this = this;
                @this.Set(ref _CurrentPage, value, PropertyChanged);
                CurrentPage.PropertyChanged += CurrentPagePropertyChangedHandler;
            }
        }

        public void Navigate(Page page)
        {
            (Application.Current.MainWindow as NavigationWindow).Navigate(page);
            if (page.DataContext is IPageViewModel viewModel)
                CurrentPage = viewModel;
            else
                CurrentPage = null;
        }

        public void Navigate<TPage>() where TPage : Page
        {
            var page = App.ServiceProvider.GetService<TPage>();
            Navigate(page);
        }
    }
}
