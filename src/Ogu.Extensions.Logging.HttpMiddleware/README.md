Ogu.Extensions.Logging.HttpMiddleware enhances logging capabilities within ASP.NET Core applications by providing middleware <small>(Similar to Serilog's <sup><sub>[RequestLoggingMiddleware.cs](https://github.com/serilog/serilog-aspnetcore/blob/dev/src/Serilog.AspNetCore/AspNetCore/RequestLoggingMiddleware.cs)</sub></sup>

## Usage

**Registration:**
```csharp
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
});
```

output

```bash
[2024-05-23T14:10:30.3705202Z-Dev-inf]: Ogu.Extensions.Logging.HttpLoggingMiddleware
                                        GET /api/samples responded 200 (OK) in 21.7721ms
```

## Enrich CorrelationId (Extra)

```csharp
app.UseHttpLoggingMiddleware(opts =>
{
    opts.EnrichLoggingContextOnResponseStarting = (loggingContext, httpContext) =>
    {
        httpContext.AddCorrelationIdHeaderIfMissing(loggingContext);
    };
});
```

```bash
[2024-05-27T23:19:33.0434168Z-Dev-inf]: Ogu.Extensions.Logging.HttpLoggingMiddleware
[f2cd6f70-9c58-4af0-a3dd-e05571404889]  GET /api/Samples responded 200 (OK) in 28.5559ms
```

## Sample Application
A sample application demonstrating the usage of Ogu.Extensions.Logging.HttpClient & Ogu.Extensions.Logging.HttpMiddleware be found [here](https://github.com/ogulcanturan/Ogu.Extensions.Logging/tree/master/samples/SampleHttp.Api).

**Links:**
- [GitHub](https://github.com/ogulcanturan/Ogu.Extensions.Logging)
- [Documentation](https://github.com/ogulcanturan/Ogu.Extensions.Logging#readme)
