using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ocelot.Configuration
{
    public class InternalConfiguration : IInternalConfiguration
    {
        public InternalConfiguration(
            List<ReRoute> reRoutes, 
            string administrationPath, 
            ServiceProviderConfiguration serviceProviderConfiguration, 
            string requestId, 
            LoadBalancerOptions loadBalancerOptions, 
            string downstreamScheme, 
            QoSOptions qoSOptions, 
            HttpHandlerOptions httpHandlerOptions)
        {
            // We do this once here so that the finder doesn't have to resort with every request
            ReRoutes = reRoutes
                ?.OrderByDescending(r =>
                    r?.UpstreamTemplatePattern?.Priority) // Make sure we select highest priority first
                .ThenByDescending(r => r.UpstreamHost) // Select not null hosts before null
                .ToImmutableList();
            AdministrationPath = administrationPath;
            ServiceProviderConfiguration = serviceProviderConfiguration;
            RequestId = requestId;
            LoadBalancerOptions = loadBalancerOptions;
            DownstreamScheme = downstreamScheme;
            QoSOptions = qoSOptions;
            HttpHandlerOptions = httpHandlerOptions;
        }

        public ImmutableList<ReRoute> ReRoutes { get; }

        public string AdministrationPath { get; }

        public ServiceProviderConfiguration ServiceProviderConfiguration { get; }

        public string RequestId { get; }

        public LoadBalancerOptions LoadBalancerOptions { get; }

        public string DownstreamScheme { get; }

        public QoSOptions QoSOptions { get; }

        public HttpHandlerOptions HttpHandlerOptions { get; }
    }
}
