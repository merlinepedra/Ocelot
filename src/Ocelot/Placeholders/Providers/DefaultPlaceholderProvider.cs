using System.Collections.Generic;
using System.Linq;
using Ocelot.Middleware;

namespace Ocelot.Placeholders.Providers
{
    public class DefaultPlaceholderProvider : IPlaceholderProvider
    {
        // "default" is used when there are no other matches.
        public string PlaceholderProviderName { get; } = "default";
        
        public IEnumerable<string> GetValues(DownstreamContext context, string value)
        {
            return context.TemplatePlaceholderNameAndValues.Where(e => e.Name == $"{{{value}}}").Select(e => e.Value);
        }
    }
}
