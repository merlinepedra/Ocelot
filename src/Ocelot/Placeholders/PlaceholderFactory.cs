using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Infrastructure.Extensions;
using Ocelot.Middleware;

namespace Ocelot.Placeholders
{
    public class PlaceholderFactory : IPlaceholderFactory
    {
        private readonly Regex _placeholderPatternMatcher = new Regex(@"{[^}]*}", RegexOptions.Compiled | RegexOptions.Singleline);
        private readonly ImmutableDictionary<string, IPlaceholderProvider> _providers;
        private readonly IPlaceholderProvider _defaultProvider;
        
        public PlaceholderFactory(IEnumerable<IPlaceholderProvider> providers)
        {
            _providers = providers.ToImmutableDictionary(k => k.PlaceholderProviderName);
            _defaultProvider = _providers["default"];
        }

        public List<PlaceholderNameAndValue> GetPlaceholdersForTemplate(DownstreamContext context, string template)
        {
            var placeholders = new List<PlaceholderNameAndValue>();
            
            var matches = _placeholderPatternMatcher.Matches(template);
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
            var trimmedMatch = trim ? rawMatch.Trim('{', '}') : rawMatch;
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
