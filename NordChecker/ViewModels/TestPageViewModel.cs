using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using NordChecker.Infrastructure;
using NordChecker.Models.Settings;
using NordChecker.Services;
using NordChecker.Shared;
using NordChecker.Views;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;

namespace NordChecker.ViewModels
{
    public class TestPageViewModel : BindableBase, IPageViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private NavigationService navigationService;

        private ExportSettings _ExportSettings;
        public ExportSettings ExportSettings
        {
            get => _ExportSettings;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ExportSettings, value, PropertyChanged);
        }

        private Wrapped<ExportSettings> _ExportSettingsWrapped;

        public string Title => "title";


        private bool _CanProceed;
        public bool CanProceed
        {
            get => _CanProceed;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _CanProceed, value, PropertyChanged);
        }

        public ICommand ProceedCommand { get; }
        public ICommand CancelCommand { get; }

        public TestPageViewModel(Wrapped<ExportSettings> exportSettingsWrapped, NavigationService navigationService)
        {
            _ExportSettingsWrapped = exportSettingsWrapped;
            ExportSettings = _ExportSettingsWrapped.Instance.Clone();
            this.navigationService = navigationService;
            
            ProceedCommand = new DelegateCommand(OnProceedCommandExecuted, CanExecuteProceedCommand)
                .ObservesProperty(() => CanProceed);

            CancelCommand = new DelegateCommand(OnCancelCommandExecuted);

            ExportSettings.PropertyChanged += (sender, e) =>
            {
                Log.Warning("VM: copy ({0}) prop changed ({1})",
                    ExportSettings.GetHashCode(), e.PropertyName);
            };
        }

        private bool CanExecuteProceedCommand() => true;

        private void OnProceedCommandExecuted()
        {
            _ExportSettingsWrapped.ReplaceWith(ExportSettings);
            navigationService.Navigate<MainPage>();
        }

        private void OnCancelCommandExecuted()
        {
            navigationService.Navigate<MainPage>();
        }
    }
}
