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
        private const string RegExMatchOneOrMoreOfEverything = ".+";
        private const string RegExMatchOneOrMoreOfEverythingUntilNextForwardSlash = "[^/]+";
        private const string RegExMatchEndString = "$";
        private const string RegExIgnoreCase = "(?i)";
        private const string RegExForwardSlashOnly = "^/$";
        private const string RegExForwardSlashAndOnePlaceHolder = "^/.*";

        private readonly IPlaceholderProcessor _placeholderProcessor;

        public UpstreamTemplatePatternCreator(IPlaceholderProcessor placeholderProcessor)
        {
            _placeholderProcessor = placeholderProcessor;
        }

        public UpstreamPathTemplate Create(IReRoute reRoute)
        {
            var originalUpstreamTemplate = reRoute.UpstreamPathTemplate;

            if (originalUpstreamTemplate == "/")
            {
                return new UpstreamPathTemplate(RegExForwardSlashOnly, reRoute.Priority, false, reRoute.UpstreamPathTemplate);
            }

            var placeholders = new List<string>();

            var matches = _placeholderProcessor.Match(originalUpstreamTemplate);

            if (originalUpstreamTemplate.Substring(0, 2) == "/{" && matches.Count == 1 && originalUpstreamTemplate.Length == matches[0].Length + 1)
            {
                return new UpstreamPathTemplate(RegExForwardSlashAndOnePlaceHolder, 0, false, reRoute.UpstreamPathTemplate);
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
                placeholders.Add(match.Value);
                var isCatchAll = (match.Index > lastSlashBeforeQuery && match.Index < queryIndex) || (match.Index > queryIndex);
                upstreamTemplate.Replace(match.Value,
                    isCatchAll
                        ? RegExMatchOneOrMoreOfEverything
                        : RegExMatchOneOrMoreOfEverythingUntilNextForwardSlash);
            }

            if (upstreamTemplate[upstreamTemplate.Length - 1] == '/')
            {
                upstreamTemplate.Remove(upstreamTemplate.Length -1, 1).Append("(/|)");
            }

            var route = reRoute.ReRouteIsCaseSensitive 
                ? $"^{upstreamTemplate}{RegExMatchEndString}" 
                : $"^{RegExIgnoreCase}{upstreamTemplate}{RegExMatchEndString}";

            return new UpstreamPathTemplate(route, reRoute.Priority, containsQueryString, reRoute.UpstreamPathTemplate);
        }
    }
}
