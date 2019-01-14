namespace Ocelot.Placeholders
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using DownstreamRouteFinder.UrlMatcher;
    using Infrastructure.Extensions;
    using Ocelot.Middleware;
    using Responses;

    public class PlaceholderProcessor : IPlaceholderProcessor
    {
        private readonly ImmutableDictionary<string, IPlaceholderProvider> _providers;
        private readonly IPlaceholderProvider _defaultProvider;

        public PlaceholderProcessor(IEnumerable<IPlaceholderProvider> providers)
        {
            _providers = providers.ToImmutableDictionary(k => k.PlaceholderProviderName);
            _defaultProvider = _providers["default"];
        }

        public Response<string> ProcessTemplate(DownstreamContext context, string template)
        {
            var result = new StringBuilder(template, template.Length * 2);
            foreach (var placeholder in GetPlaceholdersForTemplate(context, template))
            {
                result.Replace(placeholder.Name, placeholder.Value);
            }

            return new OkResponse<string>(result.ToString());
        }

        public List<PlaceholderNameAndValue> GetPlaceholdersForTemplate(DownstreamContext context, string template)
        {
            var placeholders = new List<PlaceholderNameAndValue>();

            var matches = template.MatchPlaceholders();
            if (matches.Count <= 0) return placeholders;

            foreach (Match match in matches)
            {
                placeholders.Add(GetValueForMatch(context, match.Value));
            }

            return placeholders;
        }

        public PlaceholderNameAndValue GetValueForMatch(DownstreamContext context, string match, bool trim = true)
        {
            return GetValuesForMatch(context, match, trim).FirstOrDefault();
        }

        public IEnumerable<PlaceholderNameAndValue> GetValuesForMatch(DownstreamContext context, string match, bool trim = true)
        {
            var rawMatch = trim ? match : $"{{{match}}}";
            var trimmedMatch = trim && rawMatch?.Length > 1 ? rawMatch.Substring(1, rawMatch.Length - 2) : rawMatch;
            var parts = trimmedMatch.Split("->", 2);
            var provider = parts.Length == 1 ? _defaultProvider : GetProvider(parts[0]);
            return provider.GetValues(context, trimmedMatch)
                .Select(p => new PlaceholderNameAndValue(rawMatch, p));
        }

        public IPlaceholderProvider GetProvider(string providerName)
        {
            return _providers.TryGetValue(providerName, out var provider) ? provider : _defaultProvider;
        }
    }
}
