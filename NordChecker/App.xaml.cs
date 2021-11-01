using HandyControl.Data;
using HandyControl.Themes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NordChecker.Models;
using NordChecker.Shared;
using NordChecker.ViewModels;
using NordChecker.Views;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json.Converters;

namespace NordChecker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ILogger FileLogger;
        public static ILogger ConsoleLogger;
        public static LoggingLevelSwitch LogLevelSwitch = new();
        public static IServiceProvider ServiceProvider { get; set; }

        private NavigationService navigationService;
        private static ContinuousDataStorage dataStorage = new();

        private static AppSettings _AppSettings;
        private static ExportSettings _ExportSettings;

        static App()
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU")
            {
                NumberFormat = new NumberFormatInfo()
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

            Utils.AllocConsole();
            FileLogger = new LoggerBuilder().SetLevelSwitch(LogLevelSwitch).AddFile().Build();
            ConsoleLogger = new LoggerBuilder().SetLevelSwitch(LogLevelSwitch).AddConsole().Build();
            Log.Logger = FileLogger.Merge(ConsoleLogger);

            _AppSettings = dataStorage.LoadOrDefault(new AppSettings());
            _ExportSettings = dataStorage.LoadOrDefault(new ExportSettings());
            
            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<ObservableCollection<Account>>();
            services.AddSingleton<NavigationService>();
            
            services.AddSingleton(_AppSettings);
            services.AddSingleton(_ExportSettings);

            services.AddSingleton<Cyclic<Proxy>>();
            services.AddSingleton<IChecker, MockChecker>();

            services.AddSingleton<ProxiesViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainPageViewModel>();
            services.AddSingleton<MainPage>();
            services.AddTransient<ExportPageViewModel>();
            services.AddTransient<ExportPage>();

            ServiceProvider = services.BuildServiceProvider();
        }

        public App()
        {
            navigationService = ServiceProvider.GetService<NavigationService>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (sender, e) =>
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (sender, e) =>
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
            
            _AppSettings.IsConsoleLoggingEnabled = Environment.GetCommandLineArgs().Contains("-logs");
            _AppSettings.IsDeveloperModeEnabled = Environment.GetCommandLineArgs().Contains("-dev");

            dataStorage.StartContinuousSync(_AppSettings, _AppSettings.ContinuousSyncInterval);
            dataStorage.StartContinuousSync(_ExportSettings, _AppSettings.ContinuousSyncInterval);
            
            var assembly = Assembly.GetEntryAssembly();
            var configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>().Configuration;
            Log.Information("{0} {1} is running in {2} configuration",
                assembly.GetName().Name,
                assembly.GetName().Version,
                configuration.ToLower());
        }

        private static void LogUnhandledException(Exception exception, string type)
        {
            if (exception.InnerException is OperationCanceledException) return;
            Log.Fatal(exception, "Unhandled {0}", type);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Log.Information("Exiting with exit code {0}\n", e.ApplicationExitCode);
            Log.CloseAndFlush();
            Utils.FreeConsole();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ServiceProvider.GetService<MainWindow>().Show();
            navigationService.Navigate<MainPage>();
        }
    }
}
