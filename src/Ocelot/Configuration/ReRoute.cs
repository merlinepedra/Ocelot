using System.Collections.Immutable;

namespace Ocelot.Configuration
{
    using System.Collections.Generic;
    using System.Net.Http;
    using Ocelot.Values;

    public class ReRoute
    {
        public ReRoute(IEnumerable<DownstreamReRoute> downstreamReRoute, 
            IEnumerable<HttpMethod> upstreamHttpMethod, 
            UpstreamPathTemplate upstreamTemplatePattern, 
            string upstreamHost,
            string aggregator)
        {
            UpstreamHost = upstreamHost;
            DownstreamReRoute = downstreamReRoute?.ToImmutableList();
            UpstreamHttpMethod = upstreamHttpMethod?.ToImmutableHashSet();
            UpstreamTemplatePattern = upstreamTemplatePattern;
            Aggregator = aggregator;
        }

        public UpstreamPathTemplate UpstreamTemplatePattern { get; }

        public ImmutableHashSet<HttpMethod> UpstreamHttpMethod { get; }

        public string UpstreamHost { get; }

        public ImmutableList<DownstreamReRoute> DownstreamReRoute { get; }

        public string Aggregator {get; }
    }
}
