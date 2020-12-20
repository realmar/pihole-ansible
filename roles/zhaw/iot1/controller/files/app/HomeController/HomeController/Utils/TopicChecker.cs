using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HomeController.Utils
{
    internal class TopicChecker
    {
        private readonly Dictionary<string, Regex> _cache = new Dictionary<string, Regex>();

        /// <summary>
        /// Does a regex check on the topics.
        /// </summary>
        /// <param name="allowedTopic">The allowed topic.</param>
        /// <param name="topic">The topic.</param>
        /// <returns><c>true</c> if the topic is valid, <c>false</c> if not.</returns>
        public bool Test(string allowedTopic, string topic)
        {
            if (_cache.TryGetValue(allowedTopic, out var regex) == false)
            {
                var realTopicRegex = allowedTopic.Replace(@"/", @"\/")
                    .Replace("+", @"[a-zA-Z0-9 _.-]*")
                    .Replace("#", @"[a-zA-Z0-9 \/_#+.-]*");

                regex = new Regex(realTopicRegex, RegexOptions.Compiled);

                _cache[allowedTopic] = regex;
            }

            var matches = regex.Matches(topic);
            return matches.ToList().Any(match => match.Value == topic);
        }
    }
}
