using NordChecker.Models;
using NordChecker.Shared;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NordChecker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public AppSettings Settings { get; set; } = new AppSettings();
        public static ILogger FileLogger;
        public static ILogger ConsoleLogger;
        public static LoggingLevelSwitch LogLevelSwitch = new LoggingLevelSwitch();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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

            Settings.IsConsoleLoggingEnabled = Environment.GetCommandLineArgs().Contains("-logs");
            Settings.IsDeveloperModeEnabled = Environment.GetCommandLineArgs().Contains("-dev");

            var assembly = Assembly.GetEntryAssembly();
            var configuration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>().Configuration;
            Log.Information("{n} {v} is running in {c} configuration",
                assembly.GetName().Name,
                assembly.GetName().Version,
                configuration.ToLower());
        }

        private void LogUnhandledException(Exception exception, string type)
        {
            Log.Fatal(exception, "Unhandled {event}", type);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Log.Information("Exiting with exit code {@code}\n", e.ApplicationExitCode);
            Log.CloseAndFlush();
            Utils.FreeConsole();
        }
    }
}
