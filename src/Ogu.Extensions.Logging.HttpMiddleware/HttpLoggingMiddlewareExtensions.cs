using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ogu.Extensions.Logging.Abstractions;
using System;

namespace Ogu.Extensions.Logging.HttpMiddleware
{
    public static class HttpLoggingMiddlewareExtensions
    {
        public static IServiceCollection AddHttpLoggingMiddleware(this IServiceCollection services, Action<HttpLoggingMiddlewareOptions> configure = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<LoggingContext>();
            services.AddSingleton<ILoggingContext>(s => s.GetRequiredService<LoggingContext>());

            services.Configure(configure ?? delegate (HttpLoggingMiddlewareOptions loggingOptions) { });

            return services;
        }

        public static IApplicationBuilder UseHttpLoggingMiddleware(this IApplicationBuilder app, Action<HttpLoggingMiddlewareOptions> configureOptions = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var httpLoggingOptions = app.ApplicationServices.GetService<IOptionsMonitor<HttpLoggingMiddlewareOptions>>() ??
                                     FakeOptionsMonitor<HttpLoggingMiddlewareOptions>.Create(new HttpLoggingMiddlewareOptions());

            var currentHttpLoggingOptions = httpLoggingOptions.CurrentValue;

            configureOptions?.Invoke(httpLoggingOptions.CurrentValue);

            if (currentHttpLoggingOptions.MessageTemplate == null)
            {
                throw new ArgumentException("MessageTemplate cannot be null.");
            }

            if (currentHttpLoggingOptions.GetLevel == null)
            {
                throw new ArgumentException("GetLevel cannot be null.");
            }

            var loggingContext = app.ApplicationServices.GetService<LoggingContext>() ?? new LoggingContext();

            return app.UseMiddleware<HttpLoggingMiddleware>(new object[2] { httpLoggingOptions, loggingContext }); ;
        }
    }
}