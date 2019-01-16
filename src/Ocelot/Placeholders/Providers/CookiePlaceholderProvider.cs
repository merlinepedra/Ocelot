namespace Ocelot.Placeholders.Providers
{
    using System.Collections.Generic;
    using System.Linq;
    using Middleware;

    public class CookiePlaceholderProvider : IPlaceholderProvider
    {
        public string PlaceholderProviderName { get; } = "cookie";

        public IEnumerable<string> GetValues(DownstreamContext context, string value)
        {
            return context.HttpContext.Request.Cookies.Where(c => c.Key == value).Select(c => c.Value);
        }
    }
}
