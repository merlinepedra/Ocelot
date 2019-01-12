using System.Collections.Generic;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Middleware;

namespace Ocelot.Placeholders
{
    public interface IPlaceholderFactory
    {
        PlaceholderNameAndValue GetValueForMatch(DownstreamContext context, string match, bool trim = true);

        IEnumerable<PlaceholderNameAndValue> GetValuesForMatch(DownstreamContext context, string match,
            bool trim = true);
        
        List<PlaceholderNameAndValue> GetPlaceholdersForTemplate(DownstreamContext context, string template);
        
        IPlaceholderProvider GetProvider(string placeHolder);
    }
}
