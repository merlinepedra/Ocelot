using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Ocelot.Configuration.File;
using Ocelot.Placeholders;
using Ocelot.Values;

namespace Ocelot.Configuration.Creator
{
    public class UpstreamTemplatePatternCreator : IUpstreamTemplatePatternCreator
    {
        private const string RegExMatchOneOrMoreOfEverything = @".+";
        private const string RegExMatchOneOrMoreOfEverythingUntilNextForwardSlash = @"[^/]+";
        private const string RegExMatchEndString = @"$";
        private const string RegExIgnoreCase = @"(?i)";
        private const string RegExForwardSlashOnly = @"^/$";
        private const string RegExMatchQueryWithoutWrapping = @"[^&?]+";
        private const string RegExForwardSlashAndOnePlaceHolder = @"^/(?<key>.*)";

        private readonly IPlaceholderProcessor _placeholderProcessor;

        public UpstreamTemplatePatternCreator(IPlaceholderProcessor placeholderProcessor)
        {
            _placeholderProcessor = placeholderProcessor;
        }

        public UpstreamPathTemplate Create(IReRoute reRoute)
        {
            var originalUpstreamTemplate = reRoute.UpstreamPathTemplate;
            var keys = new List<string>();

            if (originalUpstreamTemplate == "/")
            {
                return new UpstreamPathTemplate(RegExForwardSlashOnly, reRoute.Priority, false, reRoute.UpstreamPathTemplate, keys);
            }

            var matches = _placeholderProcessor.Match(originalUpstreamTemplate);

            if (originalUpstreamTemplate.Substring(0, 2) == "/{" && matches.Count == 1 && originalUpstreamTemplate.Length == matches[0].Length + 1)
            {
                var key = matches[0].Value.Substring(1, matches[0].Length - 2);
                keys.Add(key);
                return new UpstreamPathTemplate(RegExForwardSlashAndOnePlaceHolder.Replace("key", key), 0, false, reRoute.UpstreamPathTemplate, keys);
            }
            
            var upstreamTemplate = new StringBuilder(originalUpstreamTemplate, originalUpstreamTemplate.Length * 2);

            var containsQueryString = originalUpstreamTemplate.Contains("?");
            var queryIndex = int.MaxValue;

            if (containsQueryString)
            {
                queryIndex = originalUpstreamTemplate.IndexOf('?');
                upstreamTemplate.Replace("?", "\\?");
            }

            var lastSlashBeforeQuery = containsQueryString
                ? originalUpstreamTemplate.Substring(0, queryIndex).LastIndexOf('/')
                : originalUpstreamTemplate.LastIndexOf('/');

            foreach (Match match in matches)
            {
                var isCatchAll = match.Index > lastSlashBeforeQuery && match.Index < queryIndex;
                var isQuery = match.Index > queryIndex;
                var key = match.Value.Substring(1, match.Length - 2);
                keys.Add(key);
                string matcher;
                if (!isQuery)
                {
                    matcher = isCatchAll
                        ? RegExMatchOneOrMoreOfEverything
                        : RegExMatchOneOrMoreOfEverythingUntilNextForwardSlash;
                }
                else
                {
                    matcher = RegExMatchQueryWithoutWrapping;
                }
                upstreamTemplate.Replace(match.Value, $"(?<{key}>{matcher})");
            }

            if (upstreamTemplate[upstreamTemplate.Length - 1] == '/')
            {
                upstreamTemplate.Remove(upstreamTemplate.Length -1, 1).Append("(/|)");
            }

            var endRegex = containsQueryString ? "" : RegExMatchEndString;

            var route = reRoute.ReRouteIsCaseSensitive 
                ? $"^{upstreamTemplate}{endRegex}" 
                : $"^{RegExIgnoreCase}{upstreamTemplate}{endRegex}";

            return new UpstreamPathTemplate(route, reRoute.Priority, containsQueryString, reRoute.UpstreamPathTemplate, keys);
        }
    }
}
