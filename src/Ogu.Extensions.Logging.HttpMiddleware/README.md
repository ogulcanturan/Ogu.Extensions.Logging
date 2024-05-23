Ogu.Extensions.Logging.HttpMiddleware enhances logging capabilities within ASP.NET Core applications by providing middleware (Similar to Serilog's [RequestLoggingMiddleware](https://github.com/serilog/serilog-aspnetcore/blob/dev/src/Serilog.AspNetCore/AspNetCore/RequestLoggingMiddleware.cs)

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

**Links:**
- [GitHub](https://github.com/ogulcanturan/Ogu.Extensions.Logging)
- [Documentation](https://github.com/ogulcanturan/Ogu.Extensions.Logging#readme)
