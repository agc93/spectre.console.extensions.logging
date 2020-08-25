using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Spectre.Console.Extensions.Logging
{
    public class SpectreInlineLoggerProvider : ILoggerProvider {
        private readonly SpectreConsoleLoggerConfiguration _config;
        private readonly ConcurrentDictionary<string, SpectreInlineLogger> _loggers = new ConcurrentDictionary<string, SpectreInlineLogger>();

        public SpectreInlineLoggerProvider(SpectreConsoleLoggerConfiguration config) {
            _config = config;
        }
        public ILogger CreateLogger(string categoryName) {
            return _loggers.GetOrAdd(categoryName, name => new SpectreInlineLogger(name, _config));
        }

        public void Dispose() {
            _loggers.Clear();
        }
    }
}