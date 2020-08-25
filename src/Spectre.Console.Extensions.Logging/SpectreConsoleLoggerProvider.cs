using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Spectre.Console.Extensions.Logging
{
    public class SpectreConsoleLoggerProvider : ILoggerProvider {
        private readonly SpectreConsoleLoggerConfiguration _config;
        private readonly ConcurrentDictionary<string, SpectreConsoleLogger> _loggers = new ConcurrentDictionary<string, SpectreConsoleLogger>();

        public SpectreConsoleLoggerProvider(SpectreConsoleLoggerConfiguration config) {
            _config = config;
        }
        public ILogger CreateLogger(string categoryName) {
            return _loggers.GetOrAdd(categoryName, name => new SpectreConsoleLogger(name, _config));
        }

        public void Dispose() {
            _loggers.Clear();
        }
    }
}