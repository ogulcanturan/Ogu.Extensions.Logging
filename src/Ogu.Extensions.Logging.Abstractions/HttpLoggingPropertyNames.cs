namespace Ogu.Extensions.Logging.Abstractions
{
    internal static class HttpLoggingPropertyNames
    {
        public const string RequestProtocol = "RequestProtocol";
        public const string RequestMethod = "RequestMethod";
        public const string RequestPath = "RequestPath";
        public const string StatusCode = "StatusCode";
        public const string StatusCodeLiteral = "StatusCodeLiteral";
        public const string Elapsed = "Elapsed";
        public const string RequestDetails = "RequestDetails";
        public const string ResponseDetails = "ResponseDetails";
        public const string RouteValues = "RouteValues";
        public const string Scheme = "Scheme";
        public const string QueryString = "QueryString";
        public const string RequestBody = "RequestBody";
        public const string ResponseBody = "ResponseBody";
        public const string ContentType = "Content-Type";
        public const string ContentEncoding = "Content-Encoding";
        public const string ContentLength = "Content-Length";
    }
}