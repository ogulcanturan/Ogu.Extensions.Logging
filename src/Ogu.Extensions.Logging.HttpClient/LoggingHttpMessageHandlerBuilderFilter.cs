using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ogu.Extensions.Logging.Abstractions;
using System;

namespace Ogu.Extensions.Logging.HttpClient
{
    public sealed class LoggingHttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly IOptionsMonitor<HttpClientLoggingOptions> _loggingOptionsMonitor;
        private readonly LoggingContext _loggingContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;

        public LoggingHttpMessageHandlerBuilderFilter(IOptionsMonitor<HttpClientLoggingOptions> loggingOptions, LoggingContext loggingContext, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _loggingOptionsMonitor = loggingOptions ?? throw new ArgumentNullException(nameof(loggingOptions));
            _loggingContext = loggingContext ?? throw new ArgumentNullException(nameof(loggingContext));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

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

                builder.AdditionalHandlers.Insert(0, new LoggingScopeHttpMessageHandler(_loggingContext, _serviceProvider, builder.Name, _loggingOptionsMonitor, _loggerFactory));
            };
        }
    }
}