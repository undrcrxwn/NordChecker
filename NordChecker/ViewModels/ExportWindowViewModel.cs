﻿using Leaf.xNet;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NordChecker.Commands;
using NordChecker.Models;
using NordChecker.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NordChecker.ViewModels
{
    public class ExportWindowViewModel : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        private ExportSettings _Settings;
        public ExportSettings Settings
        {
            get => _Settings;
            set
            {
                (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Settings, value, PropertyChanged);
            }
        }

        public AccountFormatter Formatter { get; set; }

        private bool _IsWindowVisible = true;
        public bool IsWindowVisible
        {
            get => _IsWindowVisible;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsWindowVisible, value, PropertyChanged);
        }

        private string _OutputPreview;
        public string OutputPreview
        {
            get => _OutputPreview;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _OutputPreview, value, PropertyChanged);
        }

        private bool _CanProceed = false;
        public bool CanProceed
        {
            get => _CanProceed;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _CanProceed, value, PropertyChanged);
        }

        private bool _IsOperationConfirmed = false;
        public bool IsOperationConfirmed
        {
            get => _IsOperationConfirmed;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _IsOperationConfirmed, value, PropertyChanged);
        }

        #endregion

        public ICommand ChoosePathCommand { get; }

        private bool CanExecuteChoosePathCommand(object parameter) => true;

        private void OnChoosePathCommandExecuted(object parameter)
        {
            Log.Information("OnChoosePathCommandExecuted");

            Task.Run(() =>
            {
                var dialog = new CommonOpenFileDialog() { IsFolderPicker = true };
                IsWindowVisible = false;
                CommonFileDialogResult result = Application.Current.Dispatcher.Invoke(dialog.ShowDialog);
                IsWindowVisible = true;
                if (result != CommonFileDialogResult.Ok) return;

                Settings.RootPath = dialog.FileName;
            });
        }

        private static readonly Account sampleAccount;
        static ExportWindowViewModel()
        {
            sampleAccount = new Account("mitch13banks@gmail.com", "Sardine13");
            sampleAccount.UserId = 121211441;
            sampleAccount.Proxy = new Proxy(Socks4ProxyClient.Parse("81.18.34.98:47680"));
            sampleAccount.Token = "3761f993137551ac965ab71f8a4564305ddce3c8b2ecbcdac3bc30722cce4fa0";
            sampleAccount.RenewToken = "2f7bf7b922ccdfa091f4b66e30af996e8e06682921e02831e9589243014702ef";
            sampleAccount.ExpiresAt = DateTime.Now.AddDays(14);
        }

        private void UpdateSampleOutput()
        {
            Formatter.FormatScheme = Settings.FormatScheme;
            OutputPreview = Formatter.Format(sampleAccount);
        }

        private void UpdateCanProceed()
            => CanProceed = Directory.Exists(Settings.RootPath)
            && !string.IsNullOrEmpty(Settings.FormatScheme)
            && Settings.Filters.Values.Any(x => x.IsActivated);

        public ExportWindowViewModel(ExportSettings settings)
        {
            Settings = settings;
            Settings.PropertyChanged += (sender, e) =>
            {
                UpdateCanProceed();
                if (e.PropertyName == nameof(Settings.FormatScheme))
                    UpdateSampleOutput();
            };

            Formatter = new AccountFormatter();
            Formatter.AddPlaceholder("email", acc => acc.Email);
            Formatter.AddPlaceholder("password", acc => acc.Password);
            Formatter.AddPlaceholder("proxy", acc => acc.Proxy);
            Formatter.AddPlaceholder("expiration", acc => acc.ExpiresAt);
            Formatter.AddPlaceholder("services", acc => "<todo:services>");
            Formatter.AddPlaceholder("json", acc => "<todo:json>");

            UpdateSampleOutput();
            UpdateCanProceed();

            ChoosePathCommand = new LambdaCommand(OnChoosePathCommandExecuted, CanExecuteChoosePathCommand);
        }
    }
}