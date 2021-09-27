using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace NordChecker.Shared
{
    public interface INavigationService
    {
        void Navigate(object page);
        void GoBack();
    }

    public sealed class NavigationService : INavigationService
    {
        public NavigationService() { }

        public void Navigate(object page)
        {
            (Application.Current.MainWindow as NavigationWindow).Navigate(page);
        }

        public void GoBack()
        {
            (Application.Current.MainWindow as NavigationWindow).GoBack();
        }
    }
}
