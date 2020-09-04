using System;
using Microsoft.Extensions.Logging;

namespace Inline
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = LoggerFactory.Create(b =>
            {
                b.SetMinimumLevel(LogLevel.Trace).AddInlineSpectreConsole(c => { c.LogLevel = LogLevel.Trace; });
            });
            var logger = factory.CreateLogger<Program>();

            logger.LogInformation("Sample application starting up...");
            logger.LogTrace("Use a {adjective} format for logging {noun}", "familiar", "messages");
            logger.LogWarning("Your logs can have [bold italic]formatting[/]!");
            logger.LogDebug("This is doing well to not have [underline]any[/] critical errors");
            logger.LogCritical("Log your errors with all the [bold underline red on white]impact[/] they deserve!");
        }
    }
}
