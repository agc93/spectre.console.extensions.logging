using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Spectre.Console.Extensions.Logging
{
    public class SpectreConsoleLogger : BaseSpectreLogger
    {
        /* 
        / Because of the context this implementation originated in (a CLI tool)
        / this logger doesn't actually *use* the category name anywhere.
        */
        public SpectreConsoleLogger(string name, SpectreConsoleLoggerConfiguration config) : base(name, config) { }
        
        protected override string RenderLogMessage<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            var formattedLine = formatter(state, exception);

            var sb = new StringBuilder();

            if (Config.IncludePrefix)
            {
                var prefix = GetLevelMarkup(logLevel);

                sb.Append(prefix);
            }

            sb.Append(Name);

            if (Config.IncludeEventId)
            {
                sb.Append("[grey][[");
                sb.Append(eventId.Id);
                sb.Append("]][/]");
            }

            sb.AppendLine();

            sb.Append("      ");
            sb.Append(formattedLine);

            return sb.ToString();
        }

        private const string Unknown = "[italic dim grey]unkn[/]: ";
        private const string Trace = "[italic dim grey]trce[/]: ";
        private const string Debug = "[dim grey]dbug[/]: ";
        private const string Information = "[dim deepskyblue2]info[/]: ";
        private const string Warning = "[bold orange3]warn[/]: ";
        private const string Error = "[bold red]fail[/]: ";
        private const string Critical = "[bold underline red on white]crit[/]: ";

        private static string GetLevelMarkup(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => Trace,
                LogLevel.Debug => Debug,
                LogLevel.Information => Information,
                LogLevel.Warning => Warning,
                LogLevel.Error => Error,
                LogLevel.Critical => Critical,
                _ => Unknown
            };
        }
    }
}