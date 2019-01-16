namespace Ocelot.Placeholders.Providers
{
    using System.Collections.Generic;
    using System.Linq;
    using Middleware;

    public class ClaimsPlaceholderProvider : IPlaceholderProvider
    {
        public string PlaceholderProviderName { get; } = "claim";

        public IEnumerable<string> GetValues(DownstreamContext context, string value)
        {
            return context.HttpContext.User.Identity.IsAuthenticated ? 
                context.HttpContext.User.Claims.Where(c => c.Type == value).Select(c => c.Value) : 
                new string[0];
        }
    }
}
