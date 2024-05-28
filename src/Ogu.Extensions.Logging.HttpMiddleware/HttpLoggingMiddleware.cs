using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ogu.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ogu.Extensions.Logging.HttpMiddleware
{
    public sealed class HttpLoggingMiddleware : IDisposable
    {
        private readonly RequestDelegate _next;
        private readonly LoggingContext _loggingContext;
        private readonly Action<ILoggingContext, HttpContext> _enrichLoggingContextOnResponseStarting;
        private readonly Action<ILoggingContext, HttpContext> _enrichLoggingContextOnResponseStarted;
        private readonly Func<HttpContext, Exception, LogLevel> _getLevel;
        private readonly Func<HttpContext, string, string, string, long?, double, int, HashSet<string>, HashSet<string>, bool, bool, IEnumerable<KeyValuePair<string, object>>> _getMessageTemplateProperties;
        private readonly ILogger _logger;
        private MessageTemplate _messageTemplate;
        private bool _includeQueryInRequestPath;
        private bool _includeRequestBody;
        private long _maxRequestBodyLength;
        private bool _includeRequestHeaders;
        private bool _includeResponseBody;
        private long _maxResponseBodyLength;
        private bool _includeResponseHeaders;
        private HashSet<string> _redactRequestHeaders;
        private HashSet<string> _redactResponseHeaders;
        private string[] _notAllowedToLogBodyRequests;
        private Regex _exclusionRegex;
        private readonly IDisposable _optionsMonitor;

        public HttpLoggingMiddleware(RequestDelegate next, LoggingContext loggingContext, IOptionsMonitor<HttpLoggingMiddlewareOptions> loggingOptionsMonitor, ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _loggingContext = loggingContext ?? throw new ArgumentNullException(nameof(loggingContext));
            _ = loggingOptionsMonitor ?? throw new ArgumentNullException(nameof(loggingOptionsMonitor));

            var options = loggingOptionsMonitor.CurrentValue;

            _optionsMonitor = loggingOptionsMonitor.OnChange(OptionsChanged);

            _getLevel = options.GetLevel;
            _enrichLoggingContextOnResponseStarting = options.EnrichLoggingContextOnResponseStarting;
            _enrichLoggingContextOnResponseStarted = options.EnrichLoggingContextOnResponseStarted;
            _logger = options.Logger ?? loggerFactory.CreateLogger("Ogu.Extensions.Logging.HttpLoggingMiddleware");
            _messageTemplate = new MessageTemplate(options.MessageTemplate);
            _getMessageTemplateProperties = options.GetMessageTemplateProperties;
            _includeQueryInRequestPath = options.IncludeQueryInRequestPath;
            _includeRequestBody = options.IncludeRequestBody;
            _maxRequestBodyLength = options.MaxRequestBodyLength;
            _includeResponseBody = options.IncludeResponseBody;
            _maxResponseBodyLength = options.MaxResponseBodyLength;
            _includeRequestHeaders = options.IncludeRequestHeaders;
            _includeResponseHeaders = options.IncludeResponseHeaders;
            _redactRequestHeaders = options.RedactRequestHeaders == null
                ? new HashSet<string>()
                : new HashSet<string>(options.RedactRequestHeaders, StringComparer.OrdinalIgnoreCase);
            _redactResponseHeaders = options.RedactResponseHeaders == null
                ? new HashSet<string>()
                : new HashSet<string>(options.RedactResponseHeaders, StringComparer.OrdinalIgnoreCase);
            _notAllowedToLogBodyRequests = options.NotAllowedToLogBodyRequests ?? Array.Empty<string>();
            _exclusionRegex = _notAllowedToLogBodyRequests.Length > 0
                ? new Regex(string.Join("|", _notAllowedToLogBodyRequests.Select(Regex.Escape)), RegexOptions.IgnoreCase | RegexOptions.Compiled)
                : null;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var startTimestamp = Stopwatch.GetTimestamp();
            var collector = _loggingContext.Start();
            var isAllowedToLog = _exclusionRegex == null || !_exclusionRegex.IsMatch(httpContext.Request.Path.Value);

            try
            {
                _enrichLoggingContextOnResponseStarting?.Invoke(_loggingContext, httpContext);

                if (isAllowedToLog)
                {
                    var requestBodyPayload = _includeRequestBody && _maxRequestBodyLength >= httpContext.Request.ContentLength ? await ReadRequestBodyAsync(httpContext.Request).ConfigureAwait(false) : null;
                    string response = null;
                    long? responseContentLength = null;

                    if (_includeResponseBody)
                    {
                        var originalResponseBodyStream = httpContext.Response.Body;

                        using (var responseBody = new MemoryStream())
                        {
                            httpContext.Response.Body = responseBody;

                            await _next(httpContext);

                            if (responseBody.Length > 0 && _maxResponseBodyLength >= responseBody.Length)
                            {
                                response = await ReadResponseBodyAsync(httpContext.Response).ConfigureAwait(false);
                            }
                            else
                            {
                                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
                            }

#if NETSTANDARD2_0
                            await responseBody.CopyToAsync(originalResponseBodyStream, 81920).ConfigureAwait(false);
#else
                            await responseBody.CopyToAsync(originalResponseBodyStream).ConfigureAwait(false);
#endif
                            responseContentLength = responseBody.Length;
                        }
                    }
                    else
                    {
                        await _next(httpContext);
                    }
                    Log(httpContext, collector, requestBodyPayload, response, responseContentLength, httpContext.Response.StatusCode, startTimestamp, true, null);
                }
                else
                {
                    await _next(httpContext);
                    Log(httpContext, collector, default, default, default, httpContext.Response.StatusCode, startTimestamp, false, null);
                }
            }
            catch (Exception ex)
                when (Log(httpContext, collector, null, null, null, 500, startTimestamp, isAllowedToLog, ex))
            {
            }
            finally
            {
                collector?.Dispose();
            }
        }

        private static async Task<string> ReadResponseBodyAsync(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);

            var responseBody = await (LoggingHelper.UnreadableContentTypes.Contains(response.ContentType)
                    ? GetBase64StringFromStream(response.Body)
                    : GetStringFromStream(response.Body));

            response.Body.Seek(0, SeekOrigin.Begin);

            return responseBody;
        }

        private static async Task<string> GetStringFromStream(Stream stream)
        {
            return await new StreamReader(stream).ReadToEndAsync().ConfigureAwait(false);
        }

        private static async Task<string> GetBase64StringFromStream(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();

            var body = request.Body;
            var buffer = new byte[Convert.ToInt64(request.ContentLength)];

            _ = await request.Body.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

            if (buffer.LongLength > buffer.Length)
            {
                _ = await request.Body.ReadAsync(buffer, buffer.Length, (int)(buffer.LongLength - buffer.Length)).ConfigureAwait(false);
            }

            var requestBody = LoggingHelper.UnreadableContentTypes.Contains(request.ContentType) 
                ? Convert.ToBase64String(buffer) : 
                Encoding.UTF8.GetString(buffer);

            body.Seek(0, SeekOrigin.Begin);

            request.Body = body;

            return requestBody;
        }

        private void OptionsChanged(HttpLoggingMiddlewareOptions options)
        {
            if (options == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(options.MessageTemplate) && _messageTemplate.Text != options.MessageTemplate)
            {
                _messageTemplate = new MessageTemplate(options.MessageTemplate);
            }

            _includeQueryInRequestPath = options.IncludeQueryInRequestPath;
            _includeRequestBody = options.IncludeRequestBody;
            _maxRequestBodyLength = options.MaxRequestBodyLength;
            _includeResponseBody = options.IncludeResponseBody;
            _maxResponseBodyLength = options.MaxResponseBodyLength;
            _includeRequestHeaders = options.IncludeRequestHeaders;
            _includeResponseHeaders = options.IncludeResponseHeaders;
            _redactRequestHeaders = options.RedactRequestHeaders == null ? new HashSet<string>() : new HashSet<string>(options.RedactRequestHeaders, StringComparer.OrdinalIgnoreCase);
            _redactResponseHeaders = options.RedactResponseHeaders == null ? new HashSet<string>() : new HashSet<string>(options.RedactResponseHeaders, StringComparer.OrdinalIgnoreCase);

            if (!options.NotAllowedToLogBodyRequests?.SequenceEqual(_notAllowedToLogBodyRequests) ?? _notAllowedToLogBodyRequests.Length > 0)
            {
                _notAllowedToLogBodyRequests = options.NotAllowedToLogBodyRequests ?? Array.Empty<string>();
                _exclusionRegex = _notAllowedToLogBodyRequests.Length > 0
                    ? new Regex(string.Join("|", _notAllowedToLogBodyRequests.Select(Regex.Escape)), RegexOptions.IgnoreCase | RegexOptions.Compiled)
                    : null;
            }
        }

        private bool Log(HttpContext httpContext, LoggingContextCollector collector, string requestBody, string responseBody, long? responseContentLength, int statusCode, long startTimestamp, bool isAllowedToLog, Exception ex)
        {
            var level = _getLevel(httpContext, ex);

            if (!_logger.IsEnabled(level))
            {
                return false;
            }

            _enrichLoggingContextOnResponseStarted?.Invoke(_loggingContext, httpContext);

            collector.Get(out var collectedProperties, out var collectedException);

            httpContext.Response.OnCompleted(() =>
            {
                var stop = Stopwatch.GetTimestamp();

                var properties = collectedProperties.Concat(_getMessageTemplateProperties(httpContext, GetPath(httpContext, _includeQueryInRequestPath), requestBody, responseBody, responseContentLength, LoggingHelper.GetElapsedMilliseconds(startTimestamp, stop), statusCode, _redactRequestHeaders, _redactResponseHeaders, isAllowedToLog && _includeRequestHeaders, isAllowedToLog && _includeResponseHeaders));

                _logger.Log(level, ex ?? collectedException, _messageTemplate, properties);

                return Task.CompletedTask;
            });

            return false;
        }

        private static string GetPath(HttpContext httpContext, bool includeQueryInRequestPath)
        {
            var requestPath = includeQueryInRequestPath
                ? httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget
                : httpContext.Features.Get<IHttpRequestFeature>()?.Path;

            return string.IsNullOrEmpty(requestPath) ? httpContext.Request.Path.ToString() : requestPath;
        }

        public void Dispose()
        {
            _optionsMonitor?.Dispose();
        }
    }
}