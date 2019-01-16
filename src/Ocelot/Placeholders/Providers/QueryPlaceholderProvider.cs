namespace Ocelot.Placeholders.Providers
{
    using System.Collections.Generic;
    using Middleware;

    public class QueryPlaceholderProvider : IPlaceholderProvider
    {
        public string PlaceholderProviderName { get; } = "query";

        public IEnumerable<string> GetValues(DownstreamContext context, string value)
        {
            return context.HttpContext.Request.Query[value].ToArray();
        }
    }
}
