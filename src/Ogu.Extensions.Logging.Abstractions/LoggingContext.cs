using System;

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
    }
}