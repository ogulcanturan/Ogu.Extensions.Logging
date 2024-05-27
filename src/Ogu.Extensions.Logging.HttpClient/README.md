Ogu.Extensions.Logging.HttpClient enhances logging capabilities for HttpClient by providing a structural logging mechanism

## Usage

**Registration:**
```csharp
services.AddHttpClientLogging(opts =>
{
    opts.IncludeQueryInRequestPath = false;
    opts.IncludeRequestBody = false;
    opts.IncludeRequestHeaders = false;
    opts.IncludeResponseBody = false;
    opts.IncludeResponseHeaders = false;
});
```

output

```bash
[2024-05-23T14:41:01.8868645Z-Dev-inf]: Ogu.Extensions.Logging.HttpClient.Exchange.Default.LogicalHandler
                                        GET https://localhost:7215/api/samples responded 200 (OK) in 803.5644ms
```

## Enrich CorrelationId (Extra) 

If Ogu.Extensions.Logging.HttpMiddleware in use ( Web-based application I recommend )

```csharp
services.AddHttpClientLogging(opts =>
{
    opts.EnrichLoggingRequest = (loggingContext, message, services) =>
    {
        message.Headers.AddCorrelationIdHeaderIfMissingWithHttpContext(loggingContext, services);
    };
});
```

Prefer below Unless, web-based application: 

```csharp
services.AddHttpClientLogging(opts =>
{
    opts.EnrichLoggingRequest = (loggingContext, message, _) =>
    {
        message.Headers.AddCorrelationIdHeaderIfMissing(loggingContext);
    };
});
```

```bash
[2024-05-27T23:28:18.4737851Z-Dev-inf]: Ogu.Extensions.Logging.HttpClient.Exchange.Default.LogicalHandler
[35b23e4f-fa9c-4173-b130-6ffe764f4a85]  GET https://localhost:7215/api/samples responded 200 (OK) in 48.9729ms
```

## Sample Application
A sample application demonstrating the usage of Ogu.Extensions.Logging.HttpClient & Ogu.Extensions.Logging.HttpMiddleware be found [here](https://github.com/ogulcanturan/Ogu.Extensions.Logging/tree/master/samples/SampleHttp.Api).

**Links:**
- [GitHub](https://github.com/ogulcanturan/Ogu.Extensions.Logging)
- [Documentation](https://github.com/ogulcanturan/Ogu.Extensions.Logging#readme)
