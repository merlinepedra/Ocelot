using System.Text.RegularExpressions;

namespace Ocelot.DownstreamRouteFinder.UrlMatcher
{
    using System.Collections.Generic;
    using System.Net.Http;
    using Configuration;
    using Finder;
    using Microsoft.AspNetCore.Http;
    using Responses;

    public class RegExUrlMatcher : IUrlPathToUrlTemplateMatcher
    {
        public Response<DownstreamRoute> Match(HttpRequest request, ReRoute reRoute)
        {
            // Method and Host must match
            if (!(reRoute.UpstreamHttpMethod.IsEmpty || // Reroute allows any method or
                  reRoute.UpstreamHttpMethod.Contains(new HttpMethod(request.Method))) || // Request is in the list of specified methods
                !(string.IsNullOrWhiteSpace(reRoute.UpstreamHost) || // Reroute allows any host or
                  reRoute.UpstreamHost.Equals(request.Host.Host))) // Hosts match
            {
                return NoMatch(request);
            }

            // URL Pattern must match
            var matchPath = reRoute.UpstreamTemplatePattern.ContainsQueryString
                ? $"{request.Path}{request.QueryString}"
                : request.Path.Value;
            var match = reRoute.UpstreamTemplatePattern.Pattern.Match(matchPath);
            if (!match.Success)
            {
                return NoMatch(request);
            }

            // Add values to result
            var urlValues = new Dictionary<string, string>();
            foreach (var key in reRoute.UpstreamTemplatePattern.Keys)
            {
                var group = match.Groups[key];
                if (!group.Success || (group.Length == 0 && request.Path.Value.Length > 1)) //make sure all placeholders were found, unless root catchall
                {
                    return NoMatch(request);
                }
                urlValues.Add(key, group.Value);
            }

            return new OkResponse<DownstreamRoute>(new DownstreamRoute(urlValues, reRoute));
        }

        private Response<DownstreamRoute> NoMatch(HttpRequest request)
        {
            return new ErrorResponse<DownstreamRoute>(new UnableToFindDownstreamRouteError(request.Path, request.Method));
        }
    }
}
