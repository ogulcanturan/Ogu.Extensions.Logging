using System;
using System.Collections.Generic;
using System.Threading;

namespace Ogu.Extensions.Logging.Abstractions
{
    public sealed class LoggingContextCollector : IDisposable
    {
        private static readonly AsyncLocal<LoggingContextCollector> LoggingCollector = new AsyncLocal<LoggingContextCollector>();

#if NET9_0_OR_GREATER
        private readonly Lock _lock = new();
#else
        private readonly object _lock = new object();
#endif
        private Exception _exception;
        private Dictionary<string, object> _properties;
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
                (_properties = _properties ?? new Dictionary<string, object>())[key] = value;
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

        public void Get(out IEnumerable<KeyValuePair<string, object>> properties)
        {
            lock (_lock)
            {
                properties = _properties ?? new Dictionary<string, object>();
            }
        }

        public void Get(out IEnumerable<KeyValuePair<string, object>> properties, out Exception exception)
        {
            lock (_lock)
            {
                properties = _properties ?? new Dictionary<string, object>();
                exception = _exception;
            }
        }

        public object Get(string propertyName)
        {
            lock (_lock)
            {
                return _properties != null && _properties.TryGetValue(propertyName, out var result) ? result : null;
            }
        }

        public Exception GetException()
        {
            lock (_lock)
            {
                return _exception;
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