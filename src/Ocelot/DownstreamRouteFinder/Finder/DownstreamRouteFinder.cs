namespace Ocelot.DownstreamRouteFinder.Finder
{
    using System.Linq;
    using Configuration;
    using Microsoft.AspNetCore.Http;
    using Responses;
    using UrlMatcher;

    public class DownstreamRouteFinder : IDownstreamRouteProvider
    {
        private readonly IUrlPathToUrlTemplateMatcher _urlMatcher;

        public DownstreamRouteFinder(IUrlPathToUrlTemplateMatcher urlMatcher)
        {
            _urlMatcher = urlMatcher;
        }

        public Response<DownstreamRoute> Get(HttpRequest request, IInternalConfiguration configuration)
        {
            // Note that we are counting on the IInternalConfiguration to have the reroutes presorted by what should be matched first
            var downstreamRoute = configuration.ReRoutes
                .Select(r => _urlMatcher.Match(request, r))
                .Where(m => !m.IsError) // Pattern must match URL from upstream
                .Select(m => m.Data)
                .FirstOrDefault(); // Since we have done proper sorting above, the first one is correct. No reason to waste CPU cycles checking all routes

            return downstreamRoute != null
                ? (Response<DownstreamRoute>) new OkResponse<DownstreamRoute>(downstreamRoute)
                : new ErrorResponse<DownstreamRoute>(new UnableToFindDownstreamRouteError(request.Path, request.Method));
        }
    }
}
