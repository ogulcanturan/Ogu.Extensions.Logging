using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ogu.Extensions.Logging.Abstractions
{
    public class MessageTemplate
    {
        public MessageTemplate(string template)
        {
            Text = template;
            Placeholders = ExtractPlaceholders(template);
        }

        public Dictionary<string, int> Placeholders { get; }
        public string Text { get; }

        private static readonly Regex PlaceholderRegex = new Regex(@"\{([^{}:]+)(?::[^{}]*)?\}", RegexOptions.Compiled);

        private static Dictionary<string, int> ExtractPlaceholders(string messageTemplate)
        {
            return PlaceholderRegex.Matches(messageTemplate)
                .Cast<Match>()
                .Select((g, index) => new { g.Groups[1].Value, index })
                .ToDictionary(i => i.Value, i => i.index);
        }
    }
}