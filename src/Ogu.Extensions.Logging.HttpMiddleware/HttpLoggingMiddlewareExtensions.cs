using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Ogu.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Net.Http.Headers;

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

            return app.UseMiddleware<HttpLoggingMiddleware>(httpLoggingOptions, loggingContext);
        }

        public static void AddCorrelationIdHeaderIfMissingWithHttpContext(this HttpRequestHeaders headers, ILoggingContext loggingContext, IServiceProvider services, string correlationIdHeaderName = LoggingConstants.CorrelationIdHeaderName)
        {
            Guid? correlationId;

            if (headers.TryGetValues(correlationIdHeaderName, out var values) && Guid.TryParse(values.First(), out var parsedValue))
            {
                correlationId = parsedValue;
            }
            else
            {
                var httpContextAccessor = services.GetService<IHttpContextAccessor>();

                string headerValue = null;

                if (httpContextAccessor?.HttpContext?.Request.Headers
                        .TryGetValue(correlationIdHeaderName, out var value) == true)
                {
                    headerValue = value.ToString();
                }

                if (Guid.TryParse(headerValue, out var parsed))
                {
                    correlationId = parsed;
                }
                else
                {
                    correlationId = Guid.NewGuid();
                    headerValue = correlationId.Value.ToString();
                }

                headers.Add(correlationIdHeaderName, headerValue);
            }

            loggingContext.Upsert(LoggingConstants.CorrelationId, correlationId.Value);
        }

        public static void AddCorrelationIdHeaderIfMissing(this HttpContext httpContext, ILoggingContext loggingContext, string correlationIdHeaderName = LoggingConstants.CorrelationIdHeaderName)
        {
            Guid? correlationId;
            string headerValue = null;

            if (httpContext.Request.Headers.TryGetValue(correlationIdHeaderName, out var value))
            {
                headerValue = value.ToString();
            }

            if (Guid.TryParse(headerValue, out var parsedValue))
            {
                correlationId = parsedValue;
                httpContext.Response.Headers[correlationIdHeaderName] = value;
            }
            else
            {
                correlationId = Guid.NewGuid();
                var correlationIdAsString = new StringValues(correlationId.Value.ToString());
                httpContext.Request.Headers[correlationIdHeaderName] = correlationIdAsString;
                httpContext.Response.Headers[correlationIdHeaderName] = correlationIdAsString;
            }

            loggingContext.Upsert(LoggingConstants.CorrelationId, correlationId.Value);
        }
    }
}