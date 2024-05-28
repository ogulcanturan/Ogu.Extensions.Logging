namespace Ogu.Extensions.Logging.Abstractions
{
    public static class LoggingConstants
    {
        public const string CorrelationId = nameof(CorrelationId);
        public const string CorrelationIdHeaderName = "X-Correlation-ID";
        public const string CallerMemberName = "CallerMemberName";
        public const string CallerFilePath = "CallerFilePath";
        public const string CallerLineNumber = "CallerLineNumber";
    }
}