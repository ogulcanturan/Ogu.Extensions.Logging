using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Serilog.Templates.Themes;

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
    }
}