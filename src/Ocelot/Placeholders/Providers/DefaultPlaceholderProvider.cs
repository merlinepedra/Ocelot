using System.Collections.Generic;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Middleware;

namespace Ocelot.Placeholders.Providers
{
    public class DefaultPlaceholderProvider : IPlaceholderProvider
    {
        public string PlaceholderProviderName { get; } = null;
        
        public IEnumerable<PlaceholderNameAndValue> ProcessReplacements(DownstreamContext context)
        {
            return context.TemplatePlaceholderNameAndValues;
        }
    }
}
