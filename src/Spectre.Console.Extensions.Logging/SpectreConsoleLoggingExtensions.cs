using System;
using Spectre.Console.Extensions.Logging;

namespace Microsoft.Extensions.Logging
{
    public static class SpectreConsoleLoggingExtensions
    {
        private static ILoggingBuilder AddSpectreConsole(this ILoggingBuilder loggingBuilder, SpectreConsoleLoggerConfiguration config = null)
        {
            loggingBuilder.AddProvider(new SpectreConsoleLoggerProvider(config ?? new SpectreConsoleLoggerConfiguration()));
            return loggingBuilder;
        }

        public static ILoggingBuilder AddSpectreConsole(this ILoggingBuilder loggingBuilder, Action<SpectreConsoleLoggerConfiguration> configure)
        {
            var config = new SpectreConsoleLoggerConfiguration();
            configure(config);
            return loggingBuilder.AddSpectreConsole(config);
        }

        private static ILoggingBuilder AddInlineSpectreConsole(this ILoggingBuilder loggingBuilder, SpectreConsoleLoggerConfiguration config = null) {
            loggingBuilder.AddProvider(new SpectreInlineLoggerProvider(config ?? new SpectreConsoleLoggerConfiguration()));
            return loggingBuilder;
        }

        public static ILoggingBuilder AddInlineSpectreConsole(this ILoggingBuilder loggingBuilder, Action<SpectreConsoleLoggerConfiguration> configure)
        {
            var config = new SpectreConsoleLoggerConfiguration();
            configure(config);
            return loggingBuilder.AddInlineSpectreConsole(config);
        }
    }
}