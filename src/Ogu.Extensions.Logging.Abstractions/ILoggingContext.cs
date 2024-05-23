using System;

namespace Ogu.Extensions.Logging.Abstractions
{
    public interface ILoggingContext
    {
        void Upsert(string propertyName, object value);
        void AddOrAggregate(Exception exception);
    }
}