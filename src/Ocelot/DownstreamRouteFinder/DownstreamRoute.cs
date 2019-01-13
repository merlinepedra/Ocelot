using System.Collections.Generic;
using System.Collections.Immutable;
using Ocelot.Configuration;
using Ocelot.DownstreamRouteFinder.UrlMatcher;

namespace Ocelot.DownstreamRouteFinder
{
    public class DownstreamRoute
    {
        public DownstreamRoute(Dictionary<string, string> upstreamValues, ReRoute reRoute)
        {
            UrlValues = upstreamValues.ToImmutableDictionary();
            ReRoute = reRoute;
        }

        public ImmutableDictionary<string, string> UrlValues{ get; }
        public ReRoute ReRoute { get; }
    }
}
