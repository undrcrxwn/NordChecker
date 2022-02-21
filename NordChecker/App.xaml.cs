using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NordChecker.Models;
using NordChecker.Shared;
using NordChecker.ViewModels;
using NordChecker.Views;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NordChecker.Models.Settings;
using NordChecker.Infrastructure;
using NordChecker.Models.Stats;
using NordChecker.Services;
using NordChecker.Services.Checker;
using NordChecker.Services.AccountFormatter;
using NordChecker.Services.Storage;
using NordChecker.Services.Threading;
using NordChecker.Shared.Collections;
using Prism.Ioc;
using Prism.Regions;
using Serilog.Events;
using Prism.Unity;

namespace NordChecker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static ILogger FileLogger;
        public static ILogger ConsoleLogger;
        public static LoggingLevelSwitch LogLevelSwitch = new();

        private static ContinuousStorage Storage;

        private static AppSettings AppSettings;
        private static ExportSettings ExportSettings;
        private static ImportSettings ImportSettings;

        static App()
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU")
            {
                NumberFormat = new NumberFormatInfo
                {
                    NumberDecimalSeparator = "."
                }
            };

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                Converters = { new StringEnumConverter(), new SolidColorBrushConverter() }
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Storage = new ContinuousStorage($"{Directory.GetCurrentDirectory()}\\data");
            AppSettings = Storage.LoadOrDefault(new AppSettings());
            ExportSettings = Storage.LoadOrDefault(new ExportSettings());
            ImportSettings = Storage.LoadOrDefault(new ImportSettings());

            base.OnStartup(e);

            WindowHelper.AllocConsole();
            FileLogger = new LoggerBuilder().SetLevelSwitch(LogLevelSwitch).UseFile().Build();
            ConsoleLogger = new LoggerBuilder().SetLevelSwitch(LogLevelSwitch).UseConsole().Build();
            Log.Logger = FileLogger.Merge(ConsoleLogger);

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                LogAndThrowUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (sender, e) =>
                LogAndThrowUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (sender, e) =>
                LogAndThrowUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");

            AppSettings.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Models.Settings.AppSettings.IsAutoSaveEnabled)
                        or nameof(Models.Settings.AppSettings.ContinuousSyncInterval):
                        RefreshSettingsAutoSave();
                        break;
                    case nameof(Models.Settings.AppSettings.IsConsoleLoggingEnabled):
                        if (AppSettings.IsConsoleLoggingEnabled)
                        {
                            WindowHelper.ShowConsole();
                            Log.Logger = Log.Logger.Merge(ConsoleLogger);
                        }
                        else
                        {
                            WindowHelper.HideConsole();
                            Log.Logger = FileLogger;
                        }
                        break;
                }
            };

            RefreshSettingsAutoSave();

            ToolTipService.ShowDurationProperty.OverrideMetadata(
                typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

            Container.Resolve<NavigationService>().NavigateContent("MainView");
        }

        protected override void OnInitialized()
        {
            var assembly = Assembly.GetEntryAssembly();
            string configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>().Configuration;
            Log.Information("{0} {1} is running in {2} configuration",
                assembly.GetName().Name,
                assembly.GetName().Version,
                configuration.ToLower());

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("ContentRegion", "MainView");
        }

        protected override void RegisterTypes(IContainerRegistry registry)
        {
            // Data
            registry.RegisterInstance(AppSettings);
            registry.RegisterInstance(ExportSettings);
            registry.RegisterInstance(ImportSettings);
            registry.RegisterSingleton<ObservableCollection<Account>>();
            registry.RegisterSingleton<Cyclic<Proxy>>();
            registry.RegisterSingleton<ComboStats>();
            registry.RegisterSingleton<ProxyStats>();

            // Services
            registry.RegisterInstance<Storage>(Storage);
            registry.RegisterSingleton<NavigationService>();
            registry.RegisterSingleton<IChecker, MockChecker>();
            registry.Register<ProxyParser>(x => new ProxyParser(ImportSettings.ProxyRegexMask));

            // ViewModels
            registry.RegisterSingleton<ProxiesViewModel>();
            registry.RegisterSingleton<MainWindowViewModel>();
            registry.RegisterSingleton<MainPageViewModel>();
            registry.RegisterSingleton<ImportProxiesPageViewModel>();
            registry.RegisterSingleton<ExportPageViewModel>();
            registry.RegisterSingleton<TestPageViewModel>();

            // Views
            registry.Register<MainWindow>();
            registry.RegisterForNavigation<MainPage>("MainView");
            registry.RegisterForNavigation<ImportProxiesPage>("ImportProxiesView");
            registry.RegisterForNavigation<ExportPage>("ExportView");
            registry.RegisterForNavigation<TestPage>("TestView");
        }

        protected override Window CreateShell() => Container.Resolve<MainWindow>();

        private static void LogAndThrowUnhandledException(Exception exception, string type)
        {
            if (exception.InnerException is OperationCanceledException) return;
            Log.Fatal(exception, "Unhandled {0}", type);
            throw exception;
        }

        private static void RefreshSettingsAutoSave()
        {
            if (Storage.IsSynchronized<AppSettings>())
                Storage.StopContinuousSync<AppSettings>();

            if (Storage.IsSynchronized<ExportSettings>())
                Storage.StopContinuousSync<ExportSettings>();

            if (Storage.IsSynchronized<ImportSettings>())
                Storage.StopContinuousSync<ImportSettings>();

            if (AppSettings.IsAutoSaveEnabled)
            {
                Storage.StartContinuousSync(() => AppSettings, AppSettings.ContinuousSyncInterval);
                Storage.StartContinuousSync(() => ExportSettings, AppSettings.ContinuousSyncInterval);
                Storage.StartContinuousSync(() => ImportSettings, AppSettings.ContinuousSyncInterval);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Exiting with exit code {0}", e.ApplicationExitCode);

            base.OnExit(e);

            if (AppSettings.IsAutoSaveEnabled)
            {
                Storage.Save(AppSettings);
                Storage.Save(ExportSettings);
                Storage.Save(ImportSettings);
            }

            Log.CloseAndFlush();
            WindowHelper.FreeConsole();
        }
    }
}
