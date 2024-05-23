using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ogu.Extensions.Logging.Abstractions
{
    public static class Helper
    {
        public static readonly IReadOnlyCollection<string> UnreadableContentTypes = new HashSet<string>()
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

        public static string BuildDetails(string title, IEnumerable<KeyValuePair<string, object>> keyValuePairs, HashSet<string> redactValues)
        {
            var keyValuePairsList = keyValuePairs.GroupBy(k => k.Key).Select(g => g.First()).ToArray();

            if (keyValuePairsList.Length == 0)
            {
                return string.Empty;
            }

            using (var pool = new StringBuilderPool())
            {
                pool.Builder.AppendLine();
                pool.Builder.AppendFormat("------{0}------{1}", title, Environment.NewLine);

                for (var i = 0; i < keyValuePairsList.Length; i++)
                {
                    var kvp = keyValuePairsList[i];
                    pool.Builder.Append(kvp.Key).Append(": ");

                    if (redactValues.Contains(kvp.Key))
                    {
                        pool.Builder.Append("[REDACTED]");
                    }
                    else
                    {
#if NETCOREAPP
                        pool.Builder.AppendJoin(", ", (IEnumerable<object>)kvp.Value);
#else
                        foreach (var value in (IEnumerable<object>)kvp.Value)
                        {
                            pool.Builder.Append(value).Append(", ");
                        }

                        pool.Builder.Remove(pool.Builder.Length - 2, 2);
#endif
                    }

                    if (keyValuePairsList.Length - 1 != i)
                    {
                        pool.Builder.AppendLine();
                    }
                }

                return pool.Builder.ToString();
            }
        }
    }
}