using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Leaf.xNet;
using Microsoft.Win32;
using NordChecker.Infrastructure;
using NordChecker.Models;
using NordChecker.Shared;
using NordChecker.Views;
using Serilog;

namespace NordChecker.ViewModels
{
    public partial class MainPageViewModel
    {
        #region StartCommand

        public ICommand StartCommand
        {
            get;
        }

        private bool CanExecuteStartCommand(object parameter) => PipelineState == PipelineState.Idle;

        private void OnStartCommandExecuted(object parameter)
        {
            Log.Information("OnStartCommandExecuted");
            progressWatch.Restart();

            PipelineState = PipelineState.Working;
            distributor.Start();
        }

        #endregion

        #region PauseCommand

        public ICommand PauseCommand
        {
            get;
        }

        private bool CanExecutePauseCommand(object parameter) => PipelineState == PipelineState.Working;

        private void OnPauseCommandExecuted(object parameter)
        {
            Log.Information("OnPauseCommandExecuted");
            progressWatch.Stop();

            PipelineState = PipelineState.Paused;
            distributor.Stop();
            tokenSource.Pause();
        }

        #endregion

        #region ContinueCommand

        public ICommand ContinueCommand
        {
            get;
        }

        private bool CanExecuteContinueCommand(object parameter) => PipelineState == PipelineState.Paused;

        private void OnContinueCommandExecuted(object parameter)
        {
            Log.Information("OnContinueCommandExecuted");
            progressWatch.Start();

            PipelineState = PipelineState.Working;
            distributor.Start();
            tokenSource.Continue();
        }

        #endregion

        #region StopCommand

        public ICommand StopCommand
        {
            get;
        }

        private bool CanExecuteStopCommand(object parameter) => PipelineState != PipelineState.Idle;

        private void OnStopCommandExecuted(object parameter)
        {
            Log.Information("OnStopCommandExecuted");
            progressWatch.Stop();

            PipelineState = PipelineState.Idle;
            distributor.Stop();
            tokenSource.Cancel();
        }

        #endregion

        #region LoadCombosCommand

        public ICommand LoadCombosCommand
        {
            get;
        }

        private void OnLoadCombosCommandExecuted(object parameter)
        {
            Log.Information("OnLoadCombosCommandExecuted");

            Task.Run(() =>
            {
                OpenFileDialog dialog = null;
                bool? dialogState = null;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    dialog = new OpenFileDialog();
                    dialog.DefaultExt = ".txt";
                    dialog.Filter = "NordVPN Combo List|*.txt|Все файлы|*.*";
                    dialogState = dialog.Show(AppSettings.IsTopMostWindow);
                });
                if (dialogState != true) return;


                Task.Factory.StartNew(() =>
                {
                    Log.Information("Reading combos from {file}", dialog.FileName);
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    string line;
                    List<Account> cache = new List<Account>();
                    using (StreamReader reader = new StreamReader(File.OpenRead(dialog.FileName)))
                    {
                        while ((line = reader.ReadLine()) != null)
                        {
                            Account account;
                            try
                            {
                                account = ComboParser.Parse(line);
                            }
                            catch
                            {
                                ComboStats.MismatchedCount++;
                                Log.Debug("Line \"{line}\" has been skipped as mismatched", line);
                                continue;
                            }

                            if (AppSettings.AreComboDuplicatesSkipped)
                            {
                                if (Accounts.Any(a => a.Credentials == account.Credentials) ||
                                    cache.Any(a => a.Credentials == account.Credentials))
                                {
                                    ComboStats.DuplicatesCount++;
                                    Log.Debug("Account {credentials} has been skipped as duplicate", account);
                                    continue;
                                }
                            }

                            cache.Add(account);
                            Log.Debug("Account {credentials} has been added to the cache", account);
                        }
                    }

                    watch.Stop();
                    Log.Information("{total} accounts have been extracted from {file} in {elapsed}ms",
                        cache.Count, dialog.FileName, watch.ElapsedMilliseconds);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (Account account in cache)
                            Accounts.Add(account);
                        lock (ComboStats.ByState)
                            ComboStats.ByState[AccountState.Unchecked] += cache.Count;
                    });
                });
            });
        }

        #endregion

        #region ClearCombosCommand

        public ICommand ClearCombosCommand
        {
            get;
        }

        private bool CanExecuteClearCombosCommand(object parameter) => PipelineState != PipelineState.Working;

        private void OnClearCombosCommandExecuted(object parameter)
        {
            Log.Information("OnClearCombosCommandExecuted");

            lock (Accounts)
                Accounts.Clear();

            lock (ComboStats)
                ComboStats.Clear();

            StopCommand.Execute(null);
        }

        #endregion

        #region StopAndClearCombosCommand

        public ICommand StopAndClearCombosCommand
        {
            get;
        }

        private bool CanExecuteStopAndClearCombosCommand(object parameter)
            => StopCommand.CanExecute(null) && ClearCombosCommand.CanExecute(null);

        private void OnStopAndClearCombosCommandExecuted(object parameter)
        {
            Log.Information("OnStopAndClearCombosCommandExecuted");

            StopCommand.Execute(null);
            ClearCombosCommand.Execute(null);
        }

        #endregion

        #region LoadProxiesCommand

        public ICommand LoadProxiesCommand
        {
            get;
        }

        private void OnLoadProxiesCommandExecuted(object parameter)
        {
            Log.Information("OnLoadProxiesCommandExecuted");

            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Window window = new LoadProxiesWindow();
                    window.Owner = Application.Current.MainWindow;
                    window.ShowDialog();

                    var result = (LoadProxiesWindowViewModel)window.DataContext;
                    if (!result.IsOperationConfirmed) return;

                    Task.Factory.StartNew(() =>
                    {
                        Log.Information("Reading {type} proxies from {file}", result.ProxyType, result.Path);
                        Stopwatch watch = new Stopwatch();
                        watch.Start();

                        string line;
                        using (StreamReader reader = new StreamReader(File.OpenRead(result.Path)))
                        {
                            lock (ProxiesViewModel.Proxies)
                            {
                                while ((line = reader.ReadLine()) != null)
                                {
                                    ProxyClient client;
                                    try
                                    {
                                        var types = Enum.GetValues(typeof(ProxyType));
                                        var type = (ProxyType)types.GetValue(new Random().Next(0, types.Length));
                                        //client = ProxyClient.Parse(result.ProxyType, line);
                                        client = ProxyClient.Parse(type, line);
                                    }
                                    catch (Exception e)
                                    {
                                        ProxiesViewModel.MismatchedCount++;
                                        Log.Error(e, "Line \"{line}\" has been skipped as mismatched", line);
                                        continue;
                                    }

                                    if (AppSettings.AreProxyDuplicatesSkipped)
                                    {
                                        if (ProxiesViewModel.Proxies.Any(p => p.Client.ToString() == client.ToString()))
                                        {
                                            ProxiesViewModel.DuplicatesCount++;
                                            Log.Debug("Proxy {proxy} has been skipped as duplicate", client);
                                            continue;
                                        }
                                    }

                                    Proxy proxy = new Proxy(client);

                                    var states = Enum.GetValues(typeof(ProxyState));
                                    proxy.State = (ProxyState)states.GetValue(new Random().Next(0, states.Length));

                                    ProxiesViewModel.Proxies.Add(proxy);
                                    ProxiesViewModel.LoadedCount++;
                                    Log.Debug("Proxy {proxy} has been added to the dispenser", client);
                                }
                            }
                        }

                        watch.Stop();
                        Log.Information("{total} proxies have been extracted from {file} in {elapsed}ms",
                            ProxiesViewModel.Proxies.Count, result.Path, watch.ElapsedMilliseconds);
                    });
                });
            });
        }

        #endregion

        #region ExportCommand

        public ICommand ExportCommand
        {
            get;
        }

        private bool CanExecuteExportCommand(object parameter) =>
            PipelineState != PipelineState.Working && Accounts.Count > 0;

        private void OnExportCommandExecuted(object parameter)
        {
            Log.Information("OnExportCommandExecuted");
            navigationService.Navigate<ExportPage>();
            navigationService.Navigating += OnNavigationServiceNavigating;
        }

        private void OnNavigationServiceNavigating(object sender, Page e)
        {
            navigationService.Navigating -= OnNavigationServiceNavigating;
            Application.Current.Dispatcher.Invoke(() =>
                ((ExportPageViewModel)navigationService.CurrentPage.DataContext)
                .StateRefreshingTimer.Stop());
        }

        #endregion

        #region CopyAccountCredentialsCommand

        public ICommand CopyAccountCredentialsCommand { get; }

        private void OnCopyAccountCredentialsCommandExecuted(object parameter)
        {
            Log.Information("OnCopyAccountCredentialsExecuted");
            var (mail, password) = SelectedAccount.Credentials;
            try
            {
                Clipboard.SetText($"{mail}:{password}");
                Log.Information("Clipboard text has been set to {0}", $"{mail}:{password}");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to write account credentials {0} to clipboard", (mail, password));
            }
        }

        #endregion

        #region RemoveAccountCommand

        public ICommand RemoveAccountCommand { get; }

        private void OnRemoveAccountCommandExecuted(object parameter)
        {
            Log.Information("OnRemoveAccountCommandExecuted");
            var account = SelectedAccount;
            account.MasterToken?.Cancel();
            lock (ComboStats.ByState)
                ComboStats.ByState[account.State]--;
            Accounts.Remove(account);
            Log.Information("{0} has been removed", account);
        }

        #endregion

        #region ContactAuthorCommand

        public ICommand ContactAuthorCommand { get; }

        private void OnContactAuthorCommandExecuted(object parameter)
        {
            Log.Information("OnContactAuthorCommandExecuted");
            try
            {
                if (IsTelegramInstalled())
                    Process.Start("cmd", "/c start tg://resolve?domain=undrcrxwn");
                else
                    Process.Start("cmd", "/c start https://t.me/undrcrxwn");
            }
            catch (Exception e)
            {
                Log.Error(e, "Cannot open Telegram URL due to {exception}", e.GetType());
            }
        }

        private static bool IsTelegramInstalled()
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKey))
            {
                foreach (string subkeyName in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkeyName))
                    {
                        try
                        {
                            if (subkey.GetValue("DisplayName").ToString().ToLower().StartsWith("telegram"))
                                return true;
                        }
                        catch { }
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
