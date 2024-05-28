using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ogu.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;

namespace Ogu.Extensions.Logging.HttpMiddleware
{
    public class HttpLoggingMiddlewareOptions
    {
        private const string DefaultRequestCompletionMessageTemplate = "{RequestMethod} {RequestPath} responded {StatusCode} ({StatusCodeLiteral}) in {Elapsed:0.0000}ms {RequestDetails}{ResponseDetails}";

        public HttpLoggingMiddlewareOptions()
        {
            GetLevel = DefaultGetLevel;
            MessageTemplate = DefaultRequestCompletionMessageTemplate;
            GetMessageTemplateProperties = DefaultGetMessageTemplateProperties;

            IncludeQueryInRequestPath = true;
            IncludeRequestBody = true;
            MaxRequestBodyLength = 10240;
            IncludeRequestHeaders = true;
            IncludeResponseBody = true;
            MaxResponseBodyLength = 10240;
            IncludeResponseHeaders = true;
            RedactRequestHeaders = Array.Empty<string>();
            RedactResponseHeaders = Array.Empty<string>();
            NotAllowedToLogBodyRequests = Array.Empty<string>();
        }

        public string MessageTemplate { get; set; }

        [JsonIgnore]
        public Func<HttpContext, Exception, LogLevel> GetLevel { get; set; }

        [JsonIgnore]
        public Action<ILoggingContext, HttpContext> EnrichLoggingContextOnResponseStarting { get; set; }

        [JsonIgnore]
        public Action<ILoggingContext, HttpContext> EnrichLoggingContextOnResponseStarted { get; set; }
        
        [JsonIgnore]
        public ILogger Logger { get; set; }

        public bool IncludeQueryInRequestPath { get; set; }
        public bool IncludeRequestBody { get; set; }
        public long MaxRequestBodyLength { get; set; }
        public bool IncludeRequestHeaders { get; set; }
        public bool IncludeResponseBody { get; set; }
        public long MaxResponseBodyLength { get; set; }
        public bool IncludeResponseHeaders { get; set; }
        public string[] RedactRequestHeaders { get; set; }
        public string[] RedactResponseHeaders { get; set; }
        public string[] NotAllowedToLogBodyRequests { get; set; }
        
        [JsonIgnore]
        public Func<HttpContext, string, string, string, long?, double, int, HashSet<string>, HashSet<string>, bool, bool, IEnumerable<KeyValuePair<string, object>>> GetMessageTemplateProperties { get; set; }

        private static LogLevel DefaultGetLevel(HttpContext ctx, Exception ex)
        {
            if (ex != null || ctx.Response.StatusCode >= 500)
                return LogLevel.Error;

            return LogLevel.Information;
        }

        private static IEnumerable<KeyValuePair<string, object>> DefaultGetMessageTemplateProperties(HttpContext httpContext, string requestPath, string requestBody, string responseBody, long? responseContentLength, double elapsedMs, int statusCode, HashSet<string> redactRequestHeaders, HashSet<string> redactResponseHeaders, bool includeRequestHeaders, bool includeResponseHeaders)
        {
            var requestDetails = Enumerable.Empty<KeyValuePair<string, object>>();
            var responseDetails = Enumerable.Empty<KeyValuePair<string, object>>();

            if (includeRequestHeaders)
            {
                requestDetails = HeaderValues(httpContext.Request.Headers)
                    .OrderBy(c => c.Key)
#if NET6_0_OR_GREATER
                    .Prepend(new KeyValuePair<string, object>("Controller & Action", httpContext.Request.RouteValues.Take(2).Select(x => x.Value).ToArray()))
#endif
                    .Prepend(new KeyValuePair<string, object>("Scheme", new object[] { httpContext.Request.Scheme }))
                    .Prepend(new KeyValuePair<string, object>("RequestProtocol", new object[] { httpContext.Request.Protocol }))
                    .Prepend(new KeyValuePair<string, object>("QueryString", new object[] { httpContext.Request.QueryString }));

            }

            if (requestBody != null)
            {
                requestDetails =
                    requestDetails
                        .Prepend(new KeyValuePair<string, object>("RequestBody", new object[] { requestBody }));
            }

            if (includeResponseHeaders)
                responseDetails = HeaderValues(httpContext.Response.Headers);

            if (responseBody != null)
            {
                responseDetails =
                    responseDetails
                        .Prepend(new KeyValuePair<string, object>("Content-Length", new object[] { responseContentLength }))
                        .OrderBy(c => c.Key)
                        .Prepend(new KeyValuePair<string, object>("ResponseBody", new object[] { responseBody }));
            }

            return new[]
            {
                new KeyValuePair<string,object>("RequestProtocol", httpContext.Request.Protocol),
                new KeyValuePair<string, object>("RequestMethod", httpContext.Request.Method),
                new KeyValuePair<string, object>("RequestPath", requestPath),
                new KeyValuePair<string, object>("StatusCode", statusCode),
                new KeyValuePair<string, object>("StatusCodeLiteral", (HttpStatusCode)statusCode),
                new KeyValuePair<string, object>("Elapsed", elapsedMs),
                new KeyValuePair<string, object>("RequestDetails", LoggingHelper.BuildDetails("RequestDetails", requestDetails, redactRequestHeaders)),
                new KeyValuePair<string, object>("ResponseDetails", LoggingHelper.BuildDetails("ResponseDetails", responseDetails, redactResponseHeaders)),
            };
        }

        private static IEnumerable<KeyValuePair<string, object>> HeaderValues(IHeaderDictionary headers) =>
            headers.Select(h => new KeyValuePair<string, object>(h.Key, h.Value));
    }
}