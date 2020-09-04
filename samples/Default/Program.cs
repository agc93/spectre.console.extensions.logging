using System;
using Microsoft.Extensions.Logging;

namespace Default
{
    class Program
    {
        private static ILogger _logger;

        static void Main(string[] args)
        {
            using (var factory = LoggerFactory.Create(b =>
            {
                b.SetMinimumLevel(LogLevel.Trace).AddSpectreConsole(c => { c.LogLevel = LogLevel.Trace; c.IncludeEventId = true; });
            }))
            {
                _logger = factory.CreateLogger("SampleCategory");
            }

            _logger.LogInformation(0, "Sample application starting up...");
            _logger.LogTrace(1234, "Use a {adjective} format for logging {noun}", "familiar", "messages");
            _logger.LogWarning(100, "Your logs can have [bold italic]formatting[/]!");
            _logger.LogDebug("This is doing well to not have [underline]any[/] critical errors");
            _logger.LogCritical(500, "Log your errors with all the [bold underline white on red]impact[/] they deserve!");
        }
    }
}
