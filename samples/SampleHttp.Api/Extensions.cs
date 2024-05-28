using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Templates;
using Serilog.Templates.Themes;
using System;

namespace SampleHttp.Api
{
    public static class Extensions
    {
        public static IHostBuilder InitializeLogger(this IHostBuilder builder, IConfiguration configuration)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithEnvironmentName()
                .Enrich.FromLogContext()
                .LogToConsole()
                .LogToFile()
                .ReadFrom.Configuration(configuration);

            Log.Logger = loggerConfiguration.CreateLogger();

            builder.UseSerilog(Log.Logger, true);

            return builder;
        }

        private static LoggerConfiguration LogToConsole(this LoggerConfiguration loggerConfiguration)
        {
            const string consoleTemplate = "[{UtcDateTime(@t):o}-{Substring(EnvironmentName,0,3)}-{@l:w3}]: {SourceContext} {Coalesce(CallerLineNumber, '')}\n{#if CorrelationId is not null}[{CorrelationId}]  {#else}{CorrelationId,-40}{#end}{@m}\n{@x}";

            var consoleExpressionTemplate = new ExpressionTemplate(consoleTemplate, theme: TemplateTheme.Code,
                applyThemeWhenOutputIsRedirected: true);

            return loggerConfiguration.WriteTo.Async(c => 
                c.Console(consoleExpressionTemplate, 
                    LogEventLevel.Information));
        }

        private static LoggerConfiguration LogToFile(this LoggerConfiguration loggerConfiguration)
        {
            const string fileOutputTemplate = "[{UtcDateTime(@t):o}-{Substring(EnvironmentName,0,3)}-{@l:w3}]: {SourceContext} {Coalesce(CallerLineNumber, '')}{#if CorrelationId is not null} [{CorrelationId}] {#end}=> {@m}\n{@x}";

            var fileOutputExpressionTemplate = new ExpressionTemplate(fileOutputTemplate);

            const string filePath = @"logs\log-.txt";

            var jsonFormatter = new JsonFormatter(renderMessage: true);

            return loggerConfiguration.WriteTo.Async(c =>
                c.File(jsonFormatter,
                    path: filePath,
                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                    rollingInterval: RollingInterval.Hour,
                    fileSizeLimitBytes: 50 * 1024 * 1024,
                    retainedFileCountLimit: 3,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    flushToDiskInterval: TimeSpan.Parse("0:00:00:05.0000000")));
        }
    }
}