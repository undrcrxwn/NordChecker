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
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using NordChecker.Data;
using NordChecker.Models.Settings;
using NordChecker.Data.Storage;
using NordChecker.Infrastructure;
using NordChecker.Models.Collections;
using NordChecker.Models.Domain;
using NordChecker.Models.Domain.Checker;

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

        private readonly NavigationService _NavigationService;
        private static readonly ContinuousStorage Storage = new($"{Directory.GetCurrentDirectory()}\\data");

        private static readonly AppSettings AppSettings;
        private static readonly ExportSettings ExportSettings;

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

            Utils.AllocConsole();
            FileLogger = new LoggerBuilder().SetLevelSwitch(LogLevelSwitch).UseFile().Build();
            ConsoleLogger = new LoggerBuilder().SetLevelSwitch(LogLevelSwitch).UseConsole().Build();
            Log.Logger = FileLogger.Merge(ConsoleLogger);

            AppSettings = Storage.LoadOrDefault(new AppSettings());
            ExportSettings = Storage.LoadOrDefault(new ExportSettings());

            ServiceCollection services = new ServiceCollection();

            services.AddSingleton<ObservableCollection<Account>>();
            services.AddSingleton<NavigationService>();

            services.AddSingleton(AppSettings);
            services.AddSingleton(ExportSettings);

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

        public App() => _NavigationService = ServiceProvider.GetService<NavigationService>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                LogAndThrowUnhandledException(e.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (sender, e) =>
                LogAndThrowUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (sender, e) =>
                LogAndThrowUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");

            AppSettings.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(AppSettings.IsAutoSaveEnabled) ||
                    e.PropertyName == nameof(AppSettings.ContinuousSyncInterval))
                    RefreshSettingsAutoSave();
            };
            RefreshSettingsAutoSave();

            var assembly = Assembly.GetEntryAssembly();
            var configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>().Configuration;
            Log.Information("{0} {1} is running in {2} configuration",
                assembly.GetName().Name,
                assembly.GetName().Version,
                configuration.ToLower());
        }

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

            if (AppSettings.IsAutoSaveEnabled)
            {
                Storage.StartContinuousSync(AppSettings, AppSettings.ContinuousSyncInterval);
                Storage.StartContinuousSync(ExportSettings, AppSettings.ContinuousSyncInterval);
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
            }

            Log.CloseAndFlush();
            Utils.FreeConsole();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ServiceProvider.GetService<MainWindow>().Show();
            _NavigationService.Navigate<MainPage>();
        }
    }
}
