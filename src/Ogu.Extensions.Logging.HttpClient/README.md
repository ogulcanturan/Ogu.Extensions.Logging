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

**Links:**
- [GitHub](https://github.com/ogulcanturan/Ogu.Extensions.Logging)
- [Documentation](https://github.com/ogulcanturan/Ogu.Extensions.Logging#readme)