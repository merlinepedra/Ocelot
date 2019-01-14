using Microsoft.AspNetCore.Http;
using Ocelot.Configuration;
using Ocelot.Responses;

namespace Ocelot.DownstreamRouteFinder.Finder
{
    public interface IDownstreamRouteProvider
    {
        Response<DownstreamRoute> Get(HttpRequest request, IInternalConfiguration configuration);
    }
}
