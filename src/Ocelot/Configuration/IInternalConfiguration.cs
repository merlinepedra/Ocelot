using System.Collections.Immutable;

namespace Ocelot.Configuration
{
    public interface IInternalConfiguration
    {
        ImmutableList<ReRoute> ReRoutes { get; }

        string AdministrationPath {get;}

        ServiceProviderConfiguration ServiceProviderConfiguration {get;}

        string RequestId {get;}

        LoadBalancerOptions LoadBalancerOptions { get; }

        string DownstreamScheme { get; }

        QoSOptions QoSOptions { get; }

        HttpHandlerOptions HttpHandlerOptions { get; }
    }
}
