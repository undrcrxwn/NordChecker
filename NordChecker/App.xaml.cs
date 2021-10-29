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
        public AppSettings AppSettings { get; set; }
        public ExportSettings ExportSettings { get; set; }
        public static ILogger FileLogger;
        public static ILogger ConsoleLogger;
        public static LoggingLevelSwitch LogLevelSwitch = new LoggingLevelSwitch();
        public static IServiceProvider ServiceProvider { get; set; }

        private NavigationService navigationService;

        static App()
        {
            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<ObservableCollection<Account>>();
            services.AddSingleton<NavigationService>();

            services.AddSingleton<AppSettings>();
            services.AddSingleton<ExportSettings>();

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
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU")
            {
                NumberFormat = new NumberFormatInfo()
                {
                    NumberDecimalSeparator = "."
                }
            };

            navigationService = ServiceProvider.GetService<NavigationService>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = { new StringEnumConverter() }
            };

            Utils.AllocConsole();
            FileLogger = new LoggerBuilder().SetLevelSwitch(LogLevelSwitch).AddFile().Build();
            ConsoleLogger = new LoggerBuilder().SetLevelSwitch(LogLevelSwitch).AddConsole().Build();
            Log.Logger = FileLogger;

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (sender, e) =>
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (sender, e) =>
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");

            AppSettings = ServiceProvider.GetService<AppSettings>();
            AppSettings.IsConsoleLoggingEnabled = Environment.GetCommandLineArgs().Contains("-logs");
            AppSettings.IsDeveloperModeEnabled = Environment.GetCommandLineArgs().Contains("-dev");

            ContinuousDataStorage storage = new ContinuousDataStorage();
            //storage.StartContinuousSync(AppSettings, TimeSpan.FromSeconds(10));
            storage.Save(AppSettings);
            
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
