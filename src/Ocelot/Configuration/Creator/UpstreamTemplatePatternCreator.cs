namespace Ocelot.Configuration.Creator
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using File;
    using Infrastructure.Extensions;
    using Values;

    public class UpstreamTemplatePatternCreator : IUpstreamTemplatePatternCreator
    {
        private const string RegExMatchEverything = @".*";
        private const string RegExMatchOneOrMoreOfEverything = @".+";
        private const string RegExMatchOneOrMoreOfEverythingUntilNextForwardSlash = @"[^/]+";
        private const string RegExMatchEndString = @"$";
        private const string RegExIgnoreCase = @"(?i)";
        private const string RegExForwardSlashOnly = @"^/$";
        private const string RegExQuerySegment = @"[^&]+";

        public UpstreamPathTemplate Create(IReRoute reRoute)
        {
            var urlParts = reRoute.UpstreamPathTemplate.Split(new []{'?'}, 2);
            var hasQuery = urlParts.Length > 1;
            var originalUpstreamTemplate = urlParts[0];
            var queryTemplate = hasQuery ? urlParts[1] : null;
            var caseSensitive = reRoute.ReRouteIsCaseSensitive;
            var priority = reRoute.Priority;

            var keys = new List<string>();

            if (originalUpstreamTemplate == "/")
            {
                return new UpstreamPathTemplate(RegExForwardSlashOnly, reRoute.Priority, false, reRoute.UpstreamPathTemplate, keys);
            }

            var upstreamTemplate = new StringBuilder(originalUpstreamTemplate, reRoute.UpstreamPathTemplate.Length * 2);

            var lastSlashBeforeQuery = originalUpstreamTemplate.LastIndexOf('/');

            foreach (Match match in originalUpstreamTemplate.MatchPlaceholders())
            {
                string matcher;
                var key = match.Value.Substring(1, match.Length - 2);
                keys.Add(key);
                if (!hasQuery && match.Length + 1 == originalUpstreamTemplate.Length)
                {
                    matcher = RegExMatchEverything;
                    caseSensitive = true;
                    priority = 0;
                }
                else
                {
                    var isCatchAll = match.Index > lastSlashBeforeQuery;
                    matcher = isCatchAll
                        ? RegExMatchOneOrMoreOfEverything
                        : RegExMatchOneOrMoreOfEverythingUntilNextForwardSlash;
                }
                upstreamTemplate.Replace(match.Value, $"(?<{key}>{matcher})");
            }

            if (upstreamTemplate[upstreamTemplate.Length - 1] == '/')
            {
                upstreamTemplate.Remove(upstreamTemplate.Length -1, 1).Append("(/|)");
            }

            if (hasQuery)
            {
                upstreamTemplate.Append("\\?");
                var querySegments = queryTemplate.Split('&');
                for (var i = 0; i < querySegments.Length; i++)
                {
                    var segment = new StringBuilder(querySegments[i]);
                    foreach (Match match in querySegments[i].MatchPlaceholders())
                    {
                        var key = match.Value.Substring(1, match.Length - 2);
                        keys.Add(key);
                        segment.Replace(match.Value, $"(?<{key}>{RegExQuerySegment})");
                    }

                    upstreamTemplate.Append(segment);
                    if (i < querySegments.Length - 1)
                    {
                        upstreamTemplate.Append("|");
                    }
                }
            }

            var endRegex = hasQuery ? "" : RegExMatchEndString;

            var route = caseSensitive 
                ? $"^{upstreamTemplate}{endRegex}" 
                : $"^{RegExIgnoreCase}{upstreamTemplate}{endRegex}";

            return new UpstreamPathTemplate(route, priority, hasQuery, reRoute.UpstreamPathTemplate, keys);
        }
    }
}
