using System;
using Microsoft.Extensions.Logging;

namespace Spectre.Console.Extensions.Logging
{
    public abstract class BaseSpectreLogger : ILogger
    {
        /* 
        / Because of the context this implementation originated in (a CLI tool)
        / this logger doesn't actually *use* the category name anywhere.
        */
        private protected readonly string Name;
        private protected readonly SpectreConsoleLoggerConfiguration Config;
        private readonly IAnsiConsole _console;

        protected BaseSpectreLogger(string name, SpectreConsoleLoggerConfiguration config)
        {
            Name = name;
            Config = config;

            var settings = config.ConsoleSettings ?? new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.Detect,
                ColorSystem = ColorSystemSupport.Detect
            };
            _console = AnsiConsole.Create(settings);
            config.ConsoleConfiguration?.Invoke(_console);
        }

        protected abstract string RenderLogMessage<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter);

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= Config.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            if (Config.EventId == 0 || Config.EventId == eventId.Id)
            {
                var formattedLine = RenderLogMessage(logLevel, eventId, state, exception, formatter);

                switch (Config.InvalidMarkup)
                {
                    case InvalidMarkupHandling.WriteAsIs:
                        try
                        {
                            _console.MarkupLine(formattedLine);
                        }
                        catch (Exception)
                        {
                            _console.WriteLine(formattedLine);
                        }

                        break;
                    case InvalidMarkupHandling.WriteAsIsAndException:
                        try
                        {
                            _console.MarkupLine(formattedLine);
                        }
                        catch (InvalidOperationException ex)
                        {
                            _console.WriteLine(formattedLine);
                            _console.WriteLine("Failed to render previous line with Spectre, details:");
                            _console.WriteLine(ex.ToString());
                        }

                        break;
                    case InvalidMarkupHandling.Throw:
                        _console.MarkupLine(formattedLine);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(Config.InvalidMarkup));
                }
            }
        }
    }
}