using System;
using Microsoft.Extensions.Logging;

namespace Spectre.Console.Extensions.Logging
{
    public class SpectreInlineLogger : Microsoft.Extensions.Logging.ILogger {
        /* 
        / Because of the context this implementation originated in (a CLI tool)
        / this logger doesn't actually *use* the category name anywhere.
        */
        private readonly string _name;
        private readonly SpectreConsoleLoggerConfiguration _config;
        private readonly IAnsiConsole _console;

        public SpectreInlineLogger(string name, SpectreConsoleLoggerConfiguration config) {
            _name = name;
            _config = config;

            var settings = config.ConsoleSettings ?? new AnsiConsoleSettings {
                Ansi = AnsiSupport.Detect,
                ColorSystem = ColorSystemSupport.Detect
            };
            _console = AnsiConsole.Create(settings);
            if (config.ConsoleConfiguration != null) {
                config.ConsoleConfiguration.Invoke(_console);
            }
        }
        public IDisposable BeginScope<TState>(TState state) {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) {
            return logLevel >= _config.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            if (!IsEnabled(logLevel)) {
                return;
            }
            if (_config.EventId == 0 || _config.EventId == eventId.Id) {
                var prefix = _config.IncludePrefix
                    ? GetLevelMarkup(logLevel)
                    : string.Empty;
                var eventIdStr = _config.IncludeEventId || logLevel == LogLevel.Trace
                    ? GetEventIdMarkup(eventId) + " "
                    : string.Empty;

                _console.MarkupLine(prefix + eventIdStr + formatter(state, exception));
            }
        }

        private string GetEventIdMarkup(EventId evId) {
            if (evId.Id == 0) {
                return string.Empty.PadRight(7);
            }
            var eventId = string.Format("{0,4:####}", evId.Id);
            return $"[dim grey][[{eventId}][/] ";
        }

        private string GetLevelMarkup(LogLevel level) {
            return level switch {
                LogLevel.Critical => "[bold red underline]| CRIT|:[/] ",
                LogLevel.Error => "[bold red]|ERROR|:[/] ",
                LogLevel.Warning => "[bold orange3]| WARN|:[/] ",
                LogLevel.Information => "[bold dim]| INFO|:[/] ",
                LogLevel.Debug => "[dim]|DEBUG|:[/] ",
                LogLevel.Trace => "[dim grey]|TRACE|:[/] ",
                _ => "| UNKN|: "
            };
        }
    }
}
