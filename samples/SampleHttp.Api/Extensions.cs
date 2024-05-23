using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Ogu.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Serilog.Templates.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace SampleHttp.Api
{
    public static class Extensions
    {
        public static IHostBuilder InitializeLogger(this IHostBuilder builder, IConfiguration configuration)
        {
            const string consoleTemplate = "[{UtcDateTime(@t):o}-{Substring(EnvironmentName,0,3)}-{@l:w3}]: {SourceContext} {Coalesce(CallerLineNumber, '')}\n{#if CorrelationId is not null}[{CorrelationId}]  {#else}{CorrelationId,-40}{#end}{@m}\n{@x}";

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithEnvironmentName()
                .Enrich.FromLogContext()
                .WriteTo.Async(
                    c => c.Console(
                        new ExpressionTemplate(consoleTemplate, theme: TemplateTheme.Code,
                            applyThemeWhenOutputIsRedirected: true), LogEventLevel.Information))
                .ReadFrom.Configuration(configuration);

            Log.Logger = loggerConfiguration.CreateLogger();

            builder.UseSerilog(Log.Logger, true);

            return builder;
        }

        public static void AddCorrelationIdHeaderIfMissing(this HttpRequestHeaders headers, ILoggingContext loggingContext, IServiceProvider services, string correlationIdHeaderName = "X-Correlation-ID")
        {
            string correlationId = null;

            if (!headers.TryGetValues(correlationIdHeaderName, out var values))
            {
                var httpContextAccessor = services.GetService<IHttpContextAccessor>();

                if (httpContextAccessor?.HttpContext != null)
                {
                    if (httpContextAccessor.HttpContext.Request.Headers.TryGetValue(correlationIdHeaderName, out var value))
                    {
                        correlationId = value;
                    }
                }

                correlationId ??= Guid.NewGuid().ToString();
                headers.Add(correlationIdHeaderName, correlationId);
            }
            else
            {
                correlationId = values.First();
            }
            
            loggingContext.Upsert("CorrelationId", correlationId);
        }

        public static void AddCorrelationIdHeaderIfMissing(this HttpContext httpContext, ILoggingContext loggingContext, string correlationIdHeaderName = "X-Correlation-ID")
        {
            string correlationId;

            if (!httpContext.Request.Headers.TryGetValue(correlationIdHeaderName, out var value))
            {
                correlationId = Guid.NewGuid().ToString();
                httpContext.Request.Headers[correlationIdHeaderName] = correlationId;
            }
            else
            {
                correlationId = value;
            }

            loggingContext.Upsert("CorrelationId", correlationId);

            if (!httpContext.Response.Headers.TryGetValue(correlationIdHeaderName, out _))
            {
                httpContext.Response.Headers.Add(new KeyValuePair<string, StringValues>(correlationIdHeaderName, correlationId));
            }
        }
    }
}
