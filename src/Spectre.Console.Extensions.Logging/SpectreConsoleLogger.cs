using System;
using Microsoft.Extensions.Logging;

namespace Spectre.Console.Extensions.Logging
{
    public class SpectreConsoleLogger : ILogger
    {
        /* 
        / Because of the context this implementation originated in (a CLI tool)
        / this logger doesn't actually *use* the category name anywhere.
        */
        private readonly string _name;
        private readonly SpectreConsoleLoggerConfiguration _config;
        private readonly IAnsiConsole _console;

        public SpectreConsoleLogger(string name, SpectreConsoleLoggerConfiguration config) {
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
                var categoryStr = _config.IncludeEventId
                    ? _name + $"[grey][[{eventId.Id}]][/]"
                    : _name;

                if (!_config.SingleLine)
                {
                    _console.MarkupLine(prefix + categoryStr);
                    _console.MarkupLine(string.Empty.PadRight(6) + formatter(state, exception));
                }
                else
                {
                    _console.MarkupLine(prefix + categoryStr + " " + formatter(state, exception));
                }
            }
        }
        private string GetLevelMarkup(LogLevel level) {
            return level switch
            {
                LogLevel.Trace => "[italic dim grey]trce[/]: ",
                LogLevel.Debug => "[dim grey]dbug[/]: ",
                LogLevel.Information => "[dim deepskyblue2]info[/]: ",
                LogLevel.Warning => "[bold orange3]warn[/]: ",
                LogLevel.Error => "[bold red]fail[/]: ",
                LogLevel.Critical => "[bold underline red on white]crit[/]: ",
                _ => throw new ArgumentOutOfRangeException(nameof(level))
            };
        }
    }
}
