using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Logging;
using System;

namespace Ogu.Extensions.Logging.HttpClient
{
    internal sealed class EmptyLoggingHttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            if (next == null) 
                throw new ArgumentNullException(nameof(next));

            return (builder) =>
            {
                next(builder);

                for (var i = builder.AdditionalHandlers.Count - 1; i >= 0; i--)
                {
                    var handler = builder.AdditionalHandlers[i];
                    if (handler is LoggingHttpMessageHandler || handler is Microsoft.Extensions.Http.Logging.LoggingScopeHttpMessageHandler)
                    {
                        builder.AdditionalHandlers.RemoveAt(i);
                    }
                }
            };
        }
    }
}