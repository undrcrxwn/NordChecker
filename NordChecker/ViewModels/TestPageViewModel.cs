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

        private ExportSettings _ExportSettingsDraft;
        public ExportSettings ExportSettingsDraft
        {
            get => _ExportSettingsDraft;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _ExportSettingsDraft, value, PropertyChanged);
        }

        private readonly ExportSettings _ExportSettings;

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

        public TestPageViewModel(ExportSettings exportSettings, NavigationService navigationService)
        {
            _ExportSettings = exportSettings;
            ExportSettingsDraft = _ExportSettings.Clone();
            this.navigationService = navigationService;
            
            ProceedCommand = new DelegateCommand(OnProceedCommandExecuted, CanExecuteProceedCommand)
                .ObservesProperty(() => CanProceed);

            CancelCommand = new DelegateCommand(OnCancelCommandExecuted);
        }

        private bool CanExecuteProceedCommand() => true;

        private void OnProceedCommandExecuted()
        {
            _ExportSettings.ReplacePropertiesWithCloned(ExportSettingsDraft);
            navigationService.NavigateContent("MainView");
        }

        private void OnCancelCommandExecuted()
        {
            navigationService.NavigateContent("MainView");
        }
    }
}
