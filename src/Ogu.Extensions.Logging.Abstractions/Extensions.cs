using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Ogu.Extensions.Logging.Abstractions
{
    public static class Extensions
    {
        public static void Log(this ILogger logger, LogLevel level, Exception ex, MessageTemplate messageTemplate, IEnumerable<KeyValuePair<string, object>> properties)
        {
            var orderedProperties = new object[messageTemplate.Placeholders.Count];
            var customProperties = new Dictionary<string, object>();

            foreach (var kvp in properties)
            {
                if (messageTemplate.Placeholders.TryGetValue(kvp.Key, out var index))
                {
                    orderedProperties[index] = kvp.Value;
                }
                else
                {
                    customProperties[kvp.Key] = kvp.Value;
                }
            }

            if (customProperties.Count > 0)
            {
                using (logger.BeginScope(customProperties))
                {
                    logger.Log(level, ex, messageTemplate.Text, orderedProperties);
                }
            }
            else
            {
                logger.Log(level, ex, messageTemplate.Text, orderedProperties);
            }
        }
    }
}
