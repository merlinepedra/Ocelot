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
                case "ip":
                    return new[] { context.HttpContext.Connection.RemoteIpAddress.ToString() };
                case "port":
                    return new[] { context.HttpContext.Connection.RemotePort.ToString() };
                case "scheme":
                    return new[] { context.HttpContext.Request.Scheme };
                case "query":
                    return new[] { context.HttpContext.Request.QueryString.Value };
                case "path":
                    return new[] { context.HttpContext.Request.Path.Value };
                default:
                    return new string[0];
            }
        }
    }
}
