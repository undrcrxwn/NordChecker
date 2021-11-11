using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Collections.Generic;
using System.Threading;

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
    
    public static class LoggerExtensions
    {        
        public static ILogger Merge(this ILogger target, ILogger logger)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Logger(target)
                .WriteTo.Logger(logger)
                .CreateLogger();
        }
    }

    public class LoggerBuilder
    {
        private readonly AnsiConsoleTheme _ConsoleTheme = new(
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

        private const string ConsoleOutputFormat = "{Timestamp:yyyy-MM-ddTHH:mm:ss.ffffffzzz} {ThreadColor} {ThreadId} [{Level:u4}] {Message:lj}{NewLine}{Exception}";
        private const string FileOutputFormat = "{Timestamp:yyyy-MM-ddTHH:mm:ss.ffffffzzz} {ThreadIcon} {ThreadId} [{Level:u4}] {Message:lj}{NewLine}{Exception}";
        private readonly LoggerConfiguration _Configuration;

        public LoggerBuilder() =>
            _Configuration = new LoggerConfiguration()
                .Enrich.With<ThreadColorEnricher>()
                .Enrich.With<ThreadIconEnricher>()
                .Enrich.With<ThreadIdEnricher>();

        public LoggerBuilder SetLevelSwitch(LoggingLevelSwitch levelSwitch)
        {
            _Configuration.MinimumLevel.ControlledBy(levelSwitch);
            return this;
        }

        public LoggerBuilder UseConsole()
        {
            _Configuration.WriteTo.Console(
                outputTemplate: ConsoleOutputFormat,
                theme: _ConsoleTheme);
            return this;
        }

        public LoggerBuilder UseFile(string path = "logs/.log")
        {
            _Configuration.WriteTo.File(path,
                rollingInterval: RollingInterval.Day,
                outputTemplate: FileOutputFormat);
            return this;
        }

        public Logger Build () =>
            _Configuration.CreateLogger();
    }
}
