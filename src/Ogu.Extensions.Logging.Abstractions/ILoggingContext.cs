using System;
using System.Collections.Generic;

namespace Ogu.Extensions.Logging.Abstractions
{
    public interface ILoggingContext
    {
        void Upsert(string propertyName, object value);
        void AddOrAggregate(Exception exception);
        void Get(out IEnumerable<KeyValuePair<string, object>> properties);
        void Get(out IEnumerable<KeyValuePair<string, object>> properties, out Exception exception);
        object Get(string propertyName);
        Exception GetException();
    }
}