using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ogu.Extensions.Logging.Abstractions
{
    public partial class MessageTemplate
    {
#if NET7_0_OR_GREATER
        [GeneratedRegex(@"\{([^{}:]+)(?::[^{}]*)?\}")]
        private static partial Regex GetPlaceholderRegex();
#else
        private static readonly Regex PlaceholderRegex =
            new Regex(@"\{([^{}:]+)(?::[^{}]*)?\}", RegexOptions.Compiled);

        private static Regex GetPlaceholderRegex() => PlaceholderRegex;
#endif

        public MessageTemplate(string template)
        {
            Text = template;
            Placeholders = ExtractPlaceholders(template);
        }

        public Dictionary<string, int> Placeholders { get; }
        public string Text { get; }

        private static Dictionary<string, int> ExtractPlaceholders(string messageTemplate)
        {
            var index = 0;

            var matches = GetPlaceholderRegex().Matches(messageTemplate);

            var placeholders = new Dictionary<string, int>(matches.Count);

            if (matches.Count == 0)
            {
                return placeholders;
            }

            foreach (Match match in matches)
            {
                placeholders[match.Groups[1].Value] = index++;
            }

            return placeholders;
        }
    }
}