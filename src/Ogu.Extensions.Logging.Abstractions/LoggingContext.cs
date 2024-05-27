using System;
using System.Collections.Generic;
using System.Linq;

namespace Ogu.Extensions.Logging.Abstractions
{
    public sealed class LoggingContext : ILoggingContext
    {
        public LoggingContextCollector Start()
        {
           return LoggingContextCollector.Start();
        }

        public void Upsert(string propertyName, object value)
        {
            LoggingContextCollector.Current?.Upsert(propertyName, value);
        }

        public void AddOrAggregate(Exception exception) => LoggingContextCollector.Current?.AddOrAggregate(exception);

        public void Get(out IEnumerable<KeyValuePair<string, object>> properties)
        {
            if (LoggingContextCollector.Current == null)
            {
                properties = Enumerable.Empty<KeyValuePair<string, object>>();
            }
            else
            {
                LoggingContextCollector.Current.Get(out properties);
            }
        }

        public void Get(out IEnumerable<KeyValuePair<string, object>> properties, out Exception exception)
        {
            if (LoggingContextCollector.Current == null)
            {
                properties = Enumerable.Empty<KeyValuePair<string, object>>();
                exception = null;
            }
            else
            {
                LoggingContextCollector.Current.Get(out properties, out exception);
            }
        }

        public object Get(string propertyName)
        {
            return LoggingContextCollector.Current?.Get(propertyName);
        }

        public Exception GetException()
        {
            return LoggingContextCollector.Current?.GetException();
        }
    }
}