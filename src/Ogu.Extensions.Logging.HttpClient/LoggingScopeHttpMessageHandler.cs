using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ogu.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ogu.Extensions.Logging.HttpClient
{
    public class LoggingScopeHttpMessageHandler : DelegatingHandler
    {
        private readonly LoggingContext _loggingContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly Action<ILoggingContext, HttpRequestMessage, IServiceProvider> _enrichLoggingRequest;
        private readonly Action<ILoggingContext, HttpResponseMessage, IServiceProvider> _enrichLoggingResponse;
        private readonly Func<HttpResponseMessage, Exception, LogLevel> _getLevel;
        private readonly Func<HttpRequestMessage, HttpResponseMessage, string, string, string, double, int, HashSet<string>, HashSet<string>, bool, bool, IEnumerable<KeyValuePair<string, object>>> _getMessageTemplateProperties;
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

        public LoggingScopeHttpMessageHandler(LoggingContext loggingContext, IServiceProvider serviceProvider, string builderName, IOptionsMonitor<HttpClientLoggingOptions> loggingOptionsMonitor, ILoggerFactory loggerFactory)
        {
            _loggingContext = loggingContext;
            _serviceProvider = serviceProvider;

            var loggerName = !string.IsNullOrEmpty(builderName) ? builderName : "Default";
            var options = loggingOptionsMonitor.CurrentValue ?? new HttpClientLoggingOptions();

            _optionsMonitor = loggingOptionsMonitor.OnChange(OptionsChanged);

            _getLevel = options.GetLevel;
            _enrichLoggingRequest = options.EnrichLoggingRequest;
            _enrichLoggingResponse = options.EnrichLoggingResponse;
            _logger = options.Logger ?? loggerFactory.CreateLogger($"Ogu.Extensions.Logging.HttpClient.Exchange.{loggerName}.LogicalHandler");
            _messageTemplate = new MessageTemplate(options.MessageTemplate);
            _getMessageTemplateProperties = options.GetMessageTemplateProperties;
            _includeQueryInRequestPath = options.IncludeQueryInRequestPath;
            _includeRequestBody = options.IncludeRequestBody;
            _maxRequestBodyLength = options.MaxRequestBodyLength;
            _includeRequestHeaders = options.IncludeRequestHeaders;
            _includeResponseBody = options.IncludeResponseBody;
            _maxResponseBodyLength = options.MaxResponseBodyLength;
            _includeResponseHeaders = options.IncludeResponseHeaders;
            _redactRequestHeaders = options.RedactRequestHeaders == null
                ? new HashSet<string>()
                : new HashSet<string>(options.RedactRequestHeaders, StringComparer.OrdinalIgnoreCase);
            _redactResponseHeaders = options.RedactResponseHeaders == null
                ? new HashSet<string>()
                : new HashSet<string>(options.RedactResponseHeaders, StringComparer.OrdinalIgnoreCase);
            _notAllowedToLogBodyRequests = options.NotAllowedToLogBodyRequests ?? Array.Empty<string>();
            _exclusionRegex = _notAllowedToLogBodyRequests.Length > 0 
                ? new Regex(string.Join("|", options.NotAllowedToLogBodyRequests.Select(Regex.Escape)), RegexOptions.IgnoreCase | RegexOptions.Compiled) 
                : null;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return Core(request, cancellationToken);
        }

        internal async Task<HttpResponseMessage> Core(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var startTimestamp = 0L;

            var collector = _loggingContext.Start();

            HttpResponseMessage responseMessage = null;

            var isAllowedToLog = _exclusionRegex == null || !_exclusionRegex.IsMatch(requestMessage.RequestUri.AbsolutePath);

            try
            {
                _enrichLoggingRequest?.Invoke(_loggingContext, requestMessage, _serviceProvider);

                startTimestamp = Stopwatch.GetTimestamp();

                responseMessage = await base.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

                var stop = Stopwatch.GetTimestamp();

                if (isAllowedToLog)
                {
                    var requestBodyPayloadTask = _includeRequestBody && requestMessage.Content != null && _maxRequestBodyLength >= requestMessage.Content.Headers.ContentLength
                        ? ReadBodyAsync(requestMessage.Content)
                        : Task.FromResult<string>(null);

                    var responseBodyPayloadTask = _includeResponseBody && (_maxResponseBodyLength >= responseMessage.Content.Headers.ContentLength || responseMessage.Headers.TransferEncodingChunked.GetValueOrDefault())
                        ? ReadBodyAsync(responseMessage.Content)
                        : Task.FromResult<string>(null);

                    await Task.WhenAll(requestBodyPayloadTask, responseBodyPayloadTask).ConfigureAwait(false);

                    Log(requestMessage, responseMessage, collector, requestBodyPayloadTask.Result, responseBodyPayloadTask.Result, (int)responseMessage.StatusCode, LoggingHelper.GetElapsedMilliseconds(startTimestamp, stop), true, null);
                }
                else
                {
                    Log(requestMessage, responseMessage, collector, null, null, (int)responseMessage.StatusCode, LoggingHelper.GetElapsedMilliseconds(startTimestamp, stop), false, null);
                }
            }
            catch (Exception ex)
                when (Log(requestMessage, responseMessage, collector, null, null, 500, LoggingHelper.GetElapsedMilliseconds(startTimestamp, Stopwatch.GetTimestamp()), isAllowedToLog, ex))
            {
            }
            finally
            {
                collector?.Dispose();
            }

            return responseMessage;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _optionsMonitor?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void OptionsChanged(HttpClientLoggingOptions options)
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
            _redactRequestHeaders = options.RedactRequestHeaders == null 
                ? new HashSet<string>() 
                : new HashSet<string>(options.RedactRequestHeaders, StringComparer.OrdinalIgnoreCase);
            _redactResponseHeaders = options.RedactResponseHeaders == null 
                ? new HashSet<string>() 
                : new HashSet<string>(options.RedactResponseHeaders, StringComparer.OrdinalIgnoreCase);

            if (!options.NotAllowedToLogBodyRequests?.SequenceEqual(_notAllowedToLogBodyRequests) ?? _notAllowedToLogBodyRequests.Length > 0)
            {
                _notAllowedToLogBodyRequests = options.NotAllowedToLogBodyRequests ?? Array.Empty<string>();
                _exclusionRegex = _notAllowedToLogBodyRequests.Length > 0 
                    ? new Regex(string.Join("|", _notAllowedToLogBodyRequests.Select(Regex.Escape)), RegexOptions.IgnoreCase | RegexOptions.Compiled) 
                    : null;
            }
        }

        public async Task<string> ReadBodyAsync(HttpContent httpContent)
        {
            return LoggingHelper.UnreadableContentTypes.Contains(httpContent.Headers.ContentType.MediaType)
                ? Convert.ToBase64String(await httpContent.ReadAsByteArrayAsync().ConfigureAwait(false))
                : await httpContent.ReadAsStringAsync().ConfigureAwait(false);
        }

        private bool Log(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage, LoggingContextCollector collector, string requestBody, string responseBody, int statusCode, double elapsedMs, bool isAllowedToLog, Exception ex)
        {
            var level = _getLevel(responseMessage, ex);

            if (!_logger.IsEnabled(level))
            {
                return false;
            }

            _enrichLoggingResponse?.Invoke(_loggingContext, responseMessage, _serviceProvider);

            collector.Get(out var collectedProperties, out var collectedException);

            var properties = collectedProperties.Concat(_getMessageTemplateProperties(requestMessage, responseMessage, GetUriString(requestMessage.RequestUri, _includeQueryInRequestPath), requestBody, responseBody, elapsedMs, statusCode, _redactRequestHeaders, _redactResponseHeaders, isAllowedToLog && _includeRequestHeaders, isAllowedToLog && _includeResponseHeaders));

            _logger.Log(level, ex ?? collectedException, _messageTemplate, properties);

            return false;
        }

        private static string GetUriString(Uri requestUri, bool includeQueryInRequestPath)
        {
            return requestUri == null
                ? null
                : requestUri.IsAbsoluteUri && includeQueryInRequestPath
                    ? requestUri.AbsoluteUri
                    : requestUri.ToString();
        }
    }
}