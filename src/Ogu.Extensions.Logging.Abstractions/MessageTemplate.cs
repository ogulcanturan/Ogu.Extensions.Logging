using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ogu.Extensions.Logging.Abstractions
{
    public class MessageTemplate
    {
        private static readonly Regex PlaceholderRegex = new Regex(@"\{([^{}:]+)(?::[^{}]*)?\}", RegexOptions.Compiled);

        public MessageTemplate(string template)
        {
            Text = template;
            Placeholders = ExtractPlaceholders(template);
        }

        public Dictionary<string, int> Placeholders { get; }
        public string Text { get; }

        private static Dictionary<string, int> ExtractPlaceholders(string messageTemplate)
        {
            return PlaceholderRegex.Matches(messageTemplate)
#if !NETSTANDARD2_1
                .Cast<Match>()
#endif
                .Select((g, index) => new { g.Groups[1].Value, index })
                .ToDictionary(i => i.Value, i => i.index);
        }
    }
}