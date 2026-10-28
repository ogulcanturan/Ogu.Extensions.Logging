using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ogu.Extensions.Logging.Abstractions
{
    public static class LoggingHelper
    {
        private const string Redacted = "[REDACTED]";
        private const string KeyValueSeparator = ": ";
        private const string EntrySeparator = ", ";
        private const string SectionDivider = "------";

        public static readonly HashSet<string> UnreadableContentTypes = new HashSet<string>
        {
            "application/zip",
            "application/x-gzip",
            "application/x-tar",
            "application/x-compress",
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/webp",
            "audio/mpeg",
            "audio/wav",
            "audio/ogg",
            "video/mp4",
            "video/quicktime",
            "video/webm"
        };

        public static double GetElapsedMilliseconds(long start, long stop) => (stop - start) * 1000 / (double)Stopwatch.Frequency;

        internal static string FormatHttpProtocol(Version version)
        {
            return $"HTTP/{version}";
        }

        public static string BuildDetails(string title, IEnumerable<KeyValuePair<string, object>> keyValuePairs, HashSet<string> redactValues)
        {
            using (var pool = new StringBuilderPool())
            {
                var seenKeys = new HashSet<string>();
                var hasEntries = false;

                foreach (var kvp in keyValuePairs)
                {
                    if (!seenKeys.Add(kvp.Key))
                    {
                        continue;
                    }

                    if (!hasEntries)
                    {
                        pool.Builder
                            .AppendLine()
                            .Append(SectionDivider)
                            .Append(title)
                            .AppendLine(SectionDivider);

                        hasEntries = true;
                    }
                    else
                    {
                        pool.Builder.AppendLine();
                    }

                    pool.Builder
                        .Append(kvp.Key)
                        .Append(KeyValueSeparator);

                    if (redactValues.Contains(kvp.Key))
                    {
                        pool.Builder.Append(Redacted);
                        continue;
                    }

                    if (kvp.Value is IEnumerable<object> values)
                    {
                        var isFirst = true;

                        foreach (var value in values)
                        {
                            if (!isFirst)
                            {
                                pool.Builder.Append(EntrySeparator);
                            }

                            pool.Builder.Append(value);
                            isFirst = false;
                        }
                    }
                    else
                    {
                        pool.Builder.Append(kvp.Value);
                    }
                }
                return hasEntries
                    ? pool.Builder.ToString()
                    : string.Empty;
            }
        }
    }
}