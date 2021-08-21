using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NordChecker.Shared
{
    public class ThreadColorEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
              "ThreadColor", $"\x1b[38;5;0m\x1b[48;5;{((Thread.CurrentThread.ManagedThreadId - 1) * 2 % 24) + 232}m_"));
        }
    }

    public class ThreadIconEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
              "ThreadIcon", (char)((Thread.CurrentThread.ManagedThreadId - 1) % ('z' - 'A') + 'A')));
        }
    }

    public class ThreadIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
              "ThreadId", Thread.CurrentThread.ManagedThreadId.ToString("D4")));
        }
    }

    public class LoggerBuilder
    {
        private static AnsiConsoleTheme consoleTheme { get; } = new AnsiConsoleTheme(
            new Dictionary<ConsoleThemeStyle, string>
            {
                [ConsoleThemeStyle.Text] = "\x1b[38;5;0007m",
                [ConsoleThemeStyle.SecondaryText] = "\x1b[38;5;0007m",
                [ConsoleThemeStyle.TertiaryText] = "\x1b[38;5;0008m",
                [ConsoleThemeStyle.Invalid] = "\x1b[38;5;0011m",
                [ConsoleThemeStyle.Null] = "\x1b[38;5;0027m",
                [ConsoleThemeStyle.Name] = "\x1b[38;5;0250m",
                [ConsoleThemeStyle.String] = "\x1b[38;5;0045m",
                [ConsoleThemeStyle.Number] = "\x1b[38;5;0200m",
                [ConsoleThemeStyle.Boolean] = "\x1b[38;5;0027m",
                [ConsoleThemeStyle.Scalar] = "\x1b[38;5;0085m",
                [ConsoleThemeStyle.LevelVerbose] = "\x1b[38;5;0008m",
                [ConsoleThemeStyle.LevelDebug] = "\x1b[38;5;0007m",
                [ConsoleThemeStyle.LevelInformation] = "\x1b[38;5;0048m",
                [ConsoleThemeStyle.LevelWarning] = "\x1b[38;5;0226m",
                [ConsoleThemeStyle.LevelError] = "\x1b[38;5;0196m",
                [ConsoleThemeStyle.LevelFatal] = "\x1b[38;5;0196m\x1b[4m"
            });

        private static readonly string consoleOutputFormat = "{Timestamp:yyyy-MM-dTHH:mm:ss.ffffffzzz} {ThreadColor} {ThreadId} [{Level:u4}] {Message:lj}{NewLine}{Exception}";
        private static readonly string fileOutputFormat = "{Timestamp:yyyy-MM-dTHH:mm:ss.ffffffzzz} {ThreadIcon} {ThreadId} [{Level:u4}] {Message:lj}{NewLine}{Exception}";
        private LoggerConfiguration configuration;

        public LoggerBuilder() =>
            configuration = new LoggerConfiguration()
                .Enrich.With<ThreadColorEnricher>()
                .Enrich.With<ThreadIconEnricher>()
                .Enrich.With<ThreadIdEnricher>();

        public LoggerBuilder AddConsole()
        {
            configuration.WriteTo.Console(LogEventLevel.Information, outputTemplate: consoleOutputFormat, theme: consoleTheme);
            return this;
        }

        public LoggerBuilder AddFileOutput()
        {
            configuration.WriteTo.File("logs/.log", LogEventLevel.Information, rollingInterval: RollingInterval.Day, outputTemplate: fileOutputFormat);
            return this;
        }

        public Logger Build () =>
            configuration.CreateLogger();
    }
}
