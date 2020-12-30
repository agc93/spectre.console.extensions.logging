using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Spectre.Console.Extensions.Logging
{
    public class SpectreInlineLogger : BaseSpectreLogger{
        
        public SpectreInlineLogger(string name, SpectreConsoleLoggerConfiguration config) : base(name, config) { }
        
        protected override string RenderLogMessage<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var formattedLine = formatter(state, exception);

            var sb = new StringBuilder();
            if (Config.IncludePrefix)
            {
                var prefix = GetLevelMarkup(logLevel);
                sb.Append(prefix);
            }
            
            if (Config.IncludeEventId)
            {
                var eventIdMarkup = GetEventIdMarkup(eventId);
                sb.Append(eventIdMarkup);
            }

            sb.Append(formattedLine);

            return sb.ToString();
        }
        
        private static string GetEventIdMarkup(EventId eventId) => eventId.Id == 0 ? EmptyEventIdMarkup : string.Format(EventIdMarkup, eventId.Id);

        private const string EmptyEventIdMarkup = "       ";
        private const string EventIdMarkup = "[dim grey][[{0,4:####}]][/] ";
        
        private const string Unknown = "| UNKN|: ";
        private const string Trace = "[dim grey]|TRACE|:[/] ";
        private const string Debug = "[dim]|DEBUG|:[/] ";
        private const string Information = "[bold dim]| INFO|:[/] ";
        private const string Warning = "[bold orange3]| WARN|:[/] ";
        private const string Error = "[bold red]|ERROR|:[/] ";
        private const string Critical = "[bold underline white on red]| CRIT|:[/] ";

        private static string GetLevelMarkup(LogLevel level) =>
            level switch {
                LogLevel.Critical => Critical,
                LogLevel.Error => Error,
                LogLevel.Warning => Warning,
                LogLevel.Information => Information,
                LogLevel.Debug => Debug,
                LogLevel.Trace => Trace,
                _ => Unknown
            };
    }
}
