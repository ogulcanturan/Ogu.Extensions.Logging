using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Ogu.Extensions.Logging.Abstractions;
using System;

namespace Ogu.Extensions.Logging.HttpClient
{
    public static class HttpClientLoggingExtensions
    {
        public static IServiceCollection AddHttpClientLogging(this IServiceCollection services, Action<HttpClientLoggingOptions> opts = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<LoggingContext>();
            services.AddSingleton<ILoggingContext>(s => s.GetRequiredService<LoggingContext>());

            services.Configure(opts ?? delegate (HttpClientLoggingOptions options) { });

            services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, LoggingHttpMessageHandlerBuilderFilter>());

            return services;
        }

        public static IServiceCollection RemoveDefaultHttpClientLogging(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, EmptyLoggingHttpMessageHandlerBuilderFilter>());
            return services;
        }
    }
}