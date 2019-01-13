using System.Collections.Generic;
using System.Linq;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Middleware;

namespace Ocelot.Placeholders.Providers
{
    public class DefaultPlaceholderProvider : IPlaceholderProvider
    {
        // "default" is used when there are no other matches.
        public string PlaceholderProviderName { get; } = "default";
        
        public IEnumerable<string> GetValues(DownstreamContext context, string value)
        {
            return context.UpstreamUrlValues
                .Where(kvp => kvp.Key == value)
                .Select(kvp => kvp.Value);
        }
    }
}
