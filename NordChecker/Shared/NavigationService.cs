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
    public sealed class NavigationService : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<Page> Navigated;

        private Page _CurrentPage;
        public Page CurrentPage
        {
            get => _CurrentPage;
            private set
            {
                (this as INotifyPropertyChangedAdvanced)
                    .Set(ref _CurrentPage, value, PropertyChanged);
                Navigated?.Invoke(this, CurrentPage);
            }
        }

        public void Navigate(Page page) => CurrentPage = page;

        public void Navigate<TPage>() where TPage : Page
        {
            CurrentPage = App.ServiceProvider.GetService<TPage>();
            Navigate(CurrentPage);
        }
    }
}
