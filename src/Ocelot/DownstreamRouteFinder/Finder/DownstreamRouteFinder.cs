namespace Ocelot.DownstreamRouteFinder.Finder
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Configuration;
    using Responses;
    using UrlMatcher;

    public class DownstreamRouteFinder : IDownstreamRouteProvider
    {
        private readonly IUrlPathToUrlTemplateMatcher _urlMatcher;

        public DownstreamRouteFinder(IUrlPathToUrlTemplateMatcher urlMatcher)
        {
            _urlMatcher = urlMatcher;
        }

        public Response<DownstreamRoute> Get(string upstreamUrlPath, string upstreamQueryString, string httpMethod, IInternalConfiguration configuration, string upstreamHost)
        {
            var downstreamRoute = configuration.ReRoutes
                .Where(r => RouteIsApplicableToThisRequest(r, httpMethod, upstreamHost))
                .OrderByDescending(r =>
                    r.UpstreamTemplatePattern.Priority) // Make sure we select highest priority first
                .ThenByDescending(r => r.UpstreamHost) // Select not null hosts before null
                .Select(r => new
                {
                    Match = _urlMatcher.Match(upstreamUrlPath, upstreamQueryString, r.UpstreamTemplatePattern),
                    ReRoute = r
                })
                .Where(m => m.Match.Data.IsMatch)
                .Select(m => GetDownstreamRoute(m.Match.Data.Match, m.ReRoute))
                .FirstOrDefault();

            return downstreamRoute != null
                ? (Response<DownstreamRoute>) new OkResponse<DownstreamRoute>(downstreamRoute)
                : new ErrorResponse<DownstreamRoute>(new UnableToFindDownstreamRouteError(upstreamUrlPath, httpMethod));
        }

        private bool RouteIsApplicableToThisRequest(ReRoute reRoute, string httpMethod, string upstreamHost)
        {
            return (reRoute.UpstreamHttpMethod.Count == 0 || reRoute.UpstreamHttpMethod.Select(x => x.Method.ToLower()).Contains(httpMethod.ToLower())) &&
                   (string.IsNullOrEmpty(reRoute.UpstreamHost) || reRoute.UpstreamHost == upstreamHost);
        }

        private DownstreamRoute GetDownstreamRoute(Match match, ReRoute reRoute)
        {
            var placeholderNameAndValues = new Dictionary<string, string>();
            foreach (var key in reRoute.UpstreamTemplatePattern.Keys)
            {
                placeholderNameAndValues.Add(key, match.Groups[key].Value);
            }
            return new DownstreamRoute(placeholderNameAndValues, reRoute);
        }
    }
}
