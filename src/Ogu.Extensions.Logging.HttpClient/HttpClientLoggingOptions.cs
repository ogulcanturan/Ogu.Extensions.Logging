using Microsoft.Extensions.Logging;
using Ogu.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Ogu.Extensions.Logging.HttpClient
{
    public class HttpClientLoggingOptions
    {
        private const string DefaultRequestCompletionMessageTemplate = "{RequestMethod} {RequestPath} responded {StatusCode} ({StatusCodeLiteral}) in {Elapsed:0.0000}ms {RequestDetails}{ResponseDetails}";

        public HttpClientLoggingOptions()
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
        public Func<HttpResponseMessage, Exception, LogLevel> GetLevel { get; set; }

        [JsonIgnore]
        public Action<ILoggingContext, HttpRequestMessage, IServiceProvider> EnrichLoggingRequest { get; set; }

        [JsonIgnore]
        public Action<ILoggingContext, HttpResponseMessage, IServiceProvider> EnrichLoggingResponse { get; set; }

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
        public Func<HttpRequestMessage, HttpResponseMessage, string, string, string, double, int, HashSet<string>, HashSet<string>, bool, bool, IEnumerable<KeyValuePair<string, object>>> GetMessageTemplateProperties { get; set; }

        private static LogLevel DefaultGetLevel(HttpResponseMessage responseMessage, Exception ex)
        {
            if (ex != null || (int)responseMessage.StatusCode >= 500)
                return LogLevel.Error;

            return LogLevel.Information;
        }

        private static IEnumerable<KeyValuePair<string, object>> DefaultGetMessageTemplateProperties(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, string requestPath, string requestBody, string responseBody, double elapsedMs, int statusCode, HashSet<string> redactRequestHeaders, HashSet<string> redactResponseHeaders, bool includeRequestHeaders, bool includeResponseHeaders)
        {
#if NET462
            var requestDetails = new List<KeyValuePair<string, object>>();
            var responseDetails = new List<KeyValuePair<string, object>>();

            if (includeRequestHeaders)
            {
                requestDetails = HeaderValues(requestMessage.Headers)
                    .Concat(HeaderValues(requestMessage.Content?.Headers))
                    .OrderBy(c => c.Key)
                    .ToList();

                requestDetails.Insert(0, new KeyValuePair<string, object>("Scheme", new object[] { requestMessage.RequestUri.Scheme }));
                requestDetails.Insert(0, new KeyValuePair<string, object>("RequestProtocol", new object[] { $"HTTP/{requestMessage.Version}" }));
                requestDetails.Insert(0, new KeyValuePair<string, object>("QueryString", new object[] { requestMessage.RequestUri.Query }));
            }

            if (requestBody != null)
            {
                requestDetails.Insert(0, new KeyValuePair<string, object>("RequestBody", new object[] { requestBody }));
            }

            if (responseMessage != null)
            {
                if (includeResponseHeaders)
                {
                    responseDetails = HeaderValues(responseMessage.Headers).ToList();
                    responseDetails.Insert(0, new KeyValuePair<string, object>("Content-Type", new object[] { responseMessage.Content.Headers.ContentType }));
                    responseDetails.Insert(0, new KeyValuePair<string, object>("Content-Encoding", new object[] { responseMessage.Content.Headers.ContentEncoding }));
                }

                if (responseBody != null)
                {
                    responseDetails.Insert(0, new KeyValuePair<string, object>("Content-Length", new object[] { responseMessage.Content.Headers.ContentLength }));
                    responseDetails = responseDetails.OrderBy(c => c.Key).ToList();
                    responseDetails.Insert(0, new KeyValuePair<string, object>("ResponseBody", new object[] { responseBody }));
                }
            }

#else
            var requestDetails = Enumerable.Empty<KeyValuePair<string, object>>();
            var responseDetails = Enumerable.Empty<KeyValuePair<string, object>>();

            if (includeRequestHeaders)
            {
                requestDetails = HeaderValues(requestMessage.Headers).Concat(HeaderValues(requestMessage.Content?.Headers))
                    .OrderBy(c => c.Key)
                    .Prepend(new KeyValuePair<string, object>("Scheme", new object[] { requestMessage.RequestUri.Scheme }))
                    .Prepend(new KeyValuePair<string, object>("RequestProtocol", new object[] { $"HTTP/{requestMessage.Version}" }))
                    .Prepend(new KeyValuePair<string, object>("QueryString", new object[] { requestMessage.RequestUri.Query }));
            }

            if (requestBody != null)
            {
                requestDetails = requestDetails
                    .Prepend(new KeyValuePair<string, object>("RequestBody", new object[] { requestBody }));
            }

            if (responseMessage != null)
            {
                if (includeResponseHeaders)
                {
                    responseDetails = HeaderValues(responseMessage.Headers)
                        .Prepend(new KeyValuePair<string, object>("Content-Type", new object[] { responseMessage.Content.Headers.ContentType }))
                        .Prepend(new KeyValuePair<string, object>("Content-Encoding", new object[] { responseMessage.Content.Headers.ContentEncoding }));
                }

                if (responseBody != null)
                {
                    responseDetails =
                        responseDetails
                            .Prepend(new KeyValuePair<string, object>("Content-Length", new object[] { responseMessage.Content.Headers.ContentLength }))
                            .OrderBy(c => c.Key)
                            .Prepend(new KeyValuePair<string, object>("ResponseBody", new object[] { responseBody }));
                }
            }
#endif

            return MessageTemplateKeyValuePairs(requestMessage, requestPath, statusCode, elapsedMs, requestDetails, redactRequestHeaders, responseDetails, redactResponseHeaders);
        }

        private static IEnumerable<KeyValuePair<string, object>> MessageTemplateKeyValuePairs(
            HttpRequestMessage requestMessage, string requestPath,
            int statusCode, double elapsedMs, IEnumerable<KeyValuePair<string, object>> requestDetails,
            HashSet<string> redactRequestHeaders, IEnumerable<KeyValuePair<string, object>> responseDetails,
            HashSet<string> redactResponseHeaders)
        {
            return new KeyValuePair<string, object>[8]
            {
                new KeyValuePair<string, object>("RequestProtocol", $"HTTP/{requestMessage.Version}"),
                new KeyValuePair<string, object>("RequestMethod", requestMessage.Method.Method),
                new KeyValuePair<string, object>("RequestPath", requestPath),
                new KeyValuePair<string, object>("StatusCode", statusCode),
                new KeyValuePair<string, object>("StatusCodeLiteral", (HttpStatusCode)statusCode),
                new KeyValuePair<string, object>("Elapsed", elapsedMs),
                new KeyValuePair<string, object>("RequestDetails", Helper.BuildDetails("RequestDetails", requestDetails, redactRequestHeaders)),
                new KeyValuePair<string, object>("ResponseDetails", Helper.BuildDetails("ResponseDetails", responseDetails, redactResponseHeaders))
            };
        }

        private static IEnumerable<KeyValuePair<string, object>> HeaderValues(HttpHeaders headers) =>
            headers?.Select(h => new KeyValuePair<string, object>(h.Key, h.Value)) ?? Enumerable.Empty<KeyValuePair<string, object>>();
    }
}