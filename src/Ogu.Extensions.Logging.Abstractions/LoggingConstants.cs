namespace Ogu.Extensions.Logging.Abstractions
{
    public static class LoggingConstants
    {
        public const string DefaultRequestCompletionMessageTemplate = "{RequestMethod} {RequestPath} responded {StatusCode} ({StatusCodeLiteral}) in {Elapsed:0.0000}ms {RequestDetails}{ResponseDetails}";
        public const string CorrelationId = nameof(CorrelationId);
        public const string CorrelationIdHeaderName = "X-Correlation-ID";
        public const string CallerMemberName = "CallerMemberName";
        public const string CallerFilePath = "CallerFilePath";
        public const string CallerLineNumber = "CallerLineNumber";
    }
}