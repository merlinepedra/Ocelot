namespace Ocelot.Placeholders.Providers
{
    using System.Collections.Generic;
    using System.Linq;
    using Middleware;

    public class HeaderPlaceholderProvider : IPlaceholderProvider
    {
        public string PlaceholderProviderName { get; } = "header";

        public IEnumerable<string> GetValues(DownstreamContext context, string value)
        {
            return context.HttpContext.Request.Headers.FirstOrDefault(h => h.Key == value).Value.ToArray();
        }
    }
}
