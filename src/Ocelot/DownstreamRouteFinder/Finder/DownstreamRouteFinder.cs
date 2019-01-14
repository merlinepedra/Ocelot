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
            var downstreamRoutes = new List<DownstreamRoute>();

            var applicableReRoutes = configuration.ReRoutes
                .Where(r => RouteIsApplicableToThisRequest(r, httpMethod, upstreamHost))
                .OrderByDescending(x => x.UpstreamTemplatePattern.Priority);

            foreach (var reRoute in applicableReRoutes)
            {
                var urlMatch = _urlMatcher.Match(upstreamUrlPath, upstreamQueryString, reRoute.UpstreamTemplatePattern);

                if (!urlMatch.Data.IsMatch) continue;
                downstreamRoutes.Add(GetPlaceholderNamesAndValues(urlMatch.Data.Match, reRoute));
            }

            if (downstreamRoutes.Any())
            {
                var notNullOption = downstreamRoutes.FirstOrDefault(x => !string.IsNullOrEmpty(x.ReRoute.UpstreamHost));
                var nullOption = downstreamRoutes.FirstOrDefault(x => string.IsNullOrEmpty(x.ReRoute.UpstreamHost));

                return notNullOption != null ? new OkResponse<DownstreamRoute>(notNullOption) : new OkResponse<DownstreamRoute>(nullOption);
            }

            return new ErrorResponse<DownstreamRoute>(new UnableToFindDownstreamRouteError(upstreamUrlPath, httpMethod));
        }

        private bool RouteIsApplicableToThisRequest(ReRoute reRoute, string httpMethod, string upstreamHost)
        {
            return (reRoute.UpstreamHttpMethod.Count == 0 || reRoute.UpstreamHttpMethod.Select(x => x.Method.ToLower()).Contains(httpMethod.ToLower())) &&
                   (string.IsNullOrEmpty(reRoute.UpstreamHost) || reRoute.UpstreamHost == upstreamHost);
        }

        private DownstreamRoute GetPlaceholderNamesAndValues(Match match, ReRoute reRoute)
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
