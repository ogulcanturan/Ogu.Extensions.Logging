using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ogu.Extensions.Logging.HttpClient;
using Ogu.Extensions.Logging.HttpMiddleware;
using SampleHttp.Api;

var builder = WebApplication.CreateBuilder(args);

// Logger provider setup (Serilog or any other provider)
builder.Host.InitializeLogger(builder.Configuration);

builder.Services.AddControllers();

// To resolve HttpClient
builder.Services.AddHttpClient();

// To get current context for adding correlationId ( extras )
builder.Services.AddHttpContextAccessor();

// Swagger related setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClientLogging
builder.Services.AddHttpClientLogging(opts =>
{
    opts.IncludeQueryInRequestPath = false;
    opts.IncludeRequestBody = false;
    opts.IncludeRequestHeaders = false;
    opts.IncludeResponseBody = false;
    opts.IncludeResponseHeaders = false;

    opts.EnrichLoggingRequest = (loggingContext, message, services) =>
    {
        // (extra)
        message.Headers.AddCorrelationIdHeaderIfMissingWithHttpContext(loggingContext, services);
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HttpLoggingMiddleware
app.UseHttpLoggingMiddleware(opts =>
{
    opts.IncludeQueryInRequestPath = false;
    opts.IncludeRequestBody = false;
    opts.IncludeRequestHeaders = false;
    opts.IncludeResponseBody = false;
    opts.IncludeResponseHeaders = false;

    opts.RedactRequestHeaders = new string[]
    {
        "Cookie"
    };

    opts.EnrichLoggingContextOnResponseStarting = (loggingContext, httpContext) =>
    {
        // (extra)
        httpContext.AddCorrelationIdHeaderIfMissing(loggingContext);
    };
});

app.UseAuthorization();

app.MapControllers();

app.Run();