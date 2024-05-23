using System;
using System.Collections.Generic;
using System.Threading;

namespace Ogu.Extensions.Logging.Abstractions
{
    public sealed class LoggingContextCollector : IDisposable
    {
        private static readonly AsyncLocal<LoggingContextCollector> LoggingCollector = new AsyncLocal<LoggingContextCollector>();

        private readonly object _lock = new object();
        private Exception _exception;
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();
        private LoggingContextCollector _collector;

        public static LoggingContextCollector Start()
        {
            var collector = new LoggingContextCollector();
            collector._collector = collector;
            LoggingCollector.Value = collector;
            return collector._collector;
        }

        public static LoggingContextCollector Current => LoggingCollector.Value?._collector;

        public void Upsert(string key, object value)
        {
            lock (_lock)
            {
                _properties[key] = value;
            }
        }

        public void AddOrAggregate(Exception exception)
        {
            lock (_lock)
            {
                _exception = _exception == null 
                    ? exception 
                    : new AggregateException(_exception, exception);
            }
        }

        public void Get(out IEnumerable<KeyValuePair<string, object>> properties, out Exception exception)
        {
            lock (_lock)
            {
                properties = _properties;
                exception = _exception;
            }
        }

        public void Dispose()
        {
            _collector = null;
            if (LoggingCollector.Value == this)
            {
                LoggingCollector.Value = null;
            }
        }
    }
}