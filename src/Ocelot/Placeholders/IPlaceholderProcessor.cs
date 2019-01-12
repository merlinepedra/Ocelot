using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Middleware;

namespace Ocelot.Placeholders
{
    public interface IPlaceholderProcessor
    {
        MatchCollection Match(string template);
        
        string ProcessTemplate(DownstreamContext context, string template);
        
        PlaceholderNameAndValue GetValueForMatch(DownstreamContext context, string match, bool trim = true);

        IEnumerable<PlaceholderNameAndValue> GetValuesForMatch(DownstreamContext context, string match,
            bool trim = true);
        
        List<PlaceholderNameAndValue> GetPlaceholdersForTemplate(DownstreamContext context, string template);
        
        IPlaceholderProvider GetProvider(string placeHolder);
    }
}
