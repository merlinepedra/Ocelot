namespace Ocelot.Placeholders.Providers
{
    using System.Collections.Generic;
    using Middleware;

    public class HttpPlaceholderProvider : IPlaceholderProvider
    {
        public string PlaceholderProviderName { get; } = "http";

        public IEnumerable<string> GetValues(DownstreamContext context, string value)
        {
            switch (value)
            {
                case "method":
                    return new[] { context.HttpContext.Request.Method };
                case "host":
                    return new[] { context.HttpContext.Request.Host.Host };
                default:
                    return new string[0];
            }
        }
    }
}
