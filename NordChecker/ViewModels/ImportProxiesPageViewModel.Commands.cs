using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using NordChecker.Models;
using NordChecker.Shared;
using Serilog;

namespace NordChecker.ViewModels
{
    public partial class ImportProxiesPageViewModel
    {
        #region ChoosePathCommand

        public ICommand ChoosePathCommand
        {
            get;
        }

        private void OnChoosePathCommandExecuted()
        {
            Log.Information("OnChoosePathCommandExecuted");

            Task.Run(() =>
            {
                var dialog = new OpenFileDialog();
                dialog.DefaultExt = ".txt";
                dialog.Filter = "NordVPN Proxy List|*.txt|Все файлы|*.*";
                if (dialog.ShowDialog() != true) return;

                FilePath = dialog.FileName;
            });
        }

        #endregion

        #region ProceedCommand

        public ICommand ProceedCommand
        {
            get;
        }

        private void OnProceedCommandExecuted()
        {
            Log.Information("OnProceedCommandExecuted");
            NavigationService.NavigateContent("MainView");

            ImportSettings.ReplacePropertiesWithCloned(ImportSettingsDraft);

            Task.Factory.StartNew(() =>
            {
                Log.Information("Reading combos from {0}", FilePath);
                var watch = new Stopwatch();
                watch.Start();

                string line;
                var cache = new List<Proxy>();
                using (var reader = new StreamReader(File.OpenRead(FilePath)))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        Proxy proxy;
                        try
                        {
                            proxy = ProxyParser.Parse(line, ImportSettings.ProxyType);
                        }
                        catch
                        {
                            ProxyStats.MismatchedCount++;
                            Log.Debug("Line \"{0}\" has been skipped as mismatched", line);
                            continue;
                        }

                        if (ImportSettings.AreComboDuplicatesSkipped)
                        {
                            if (Proxies.Any(x => x.Equals(proxy)) ||
                                cache.Any(x => x.Equals(proxy)))
                            {
                                ProxyStats.DuplicatesCount++;
                                Log.Debug("Proxy {0} has been skipped as duplicate", proxy);
                                continue;
                            }
                        }

                        cache.Add(proxy);
                        Log.Debug("Proxy {0} has been added to the cache", proxy);
                    }
                }

                watch.Stop();
                Log.Information("{0} accounts have been extracted from {1} in {2}ms",
                    cache.Count, FilePath, watch.ElapsedMilliseconds);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (Proxy proxy in cache)
                        Proxies.Add(proxy);
                    lock (ProxyStats.ByState)
                    {
                        ProxyStats.ByState[ProxyState.Unused] += cache.Count;
                        ProxyStats.ByType[ImportSettings.ProxyType] += cache.Count;
                    }
                });
            });
        }

        #endregion

        #region CancelCommand

        public ICommand CancelCommand
        {
            get;
        }

        private void OnCancelCommandExecuted()
        {
            Log.Information("OnCancelCommandExecuted");
            NavigationService.NavigateContent("MainView");
        }
        
        #endregion
    }
}
