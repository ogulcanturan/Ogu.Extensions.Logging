using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ogu.Extensions.Logging.Abstractions
{
    public static class LoggingExtensions
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

        public static IDisposable BeginScopeWithCallerInfo(this ILogger logger, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            return logger.BeginScope(CreateCallerInfo(callerMemberName, callerFilePath, callerLineNumber));
        }

        public static void LogWithCallerInfo(this ILogger logger, Action<ILogger> action, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            try
            {
                using (logger.BeginScope(CreateCallerInfo(callerMemberName, callerFilePath, callerLineNumber)))
                {
                    action(logger);
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected exception occurred");
            }
        }

        private static KeyValuePair<string, object>[] CreateCallerInfo(string callerMemberName, string callerFilePath, int callerLineNumber)
        {
            return new[]
            {
                new KeyValuePair<string, object>(LoggingConstants.CallerMemberName, callerMemberName),
                new KeyValuePair<string, object>(LoggingConstants.CallerFilePath, callerFilePath),
                new KeyValuePair<string, object>(LoggingConstants.CallerLineNumber, callerLineNumber)
            };
        }
    }
}