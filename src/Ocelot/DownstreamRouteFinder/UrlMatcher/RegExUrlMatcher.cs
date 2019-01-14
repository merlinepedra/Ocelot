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
                  reRoute.UpstreamHost.Equals(request.Headers["Host"]))) // Hosts match
            {
                return NoMatch(request);
            }

            // URL Pattern must match
            var matchPath = reRoute.UpstreamTemplatePattern.ContainsQueryString
                ? $"{request.Path}{request.QueryString}"
                : request.Path.Value;
            var match = reRoute.UpstreamTemplatePattern.Pattern.Match(matchPath);

            // Add values to result
            var urlValues = new Dictionary<string, string>();
            foreach (var key in reRoute.UpstreamTemplatePattern.Keys)
            {
                urlValues.Add(key, match.Groups[key].Value);
            }

            return match.Success
                ? new OkResponse<DownstreamRoute>(new DownstreamRoute(urlValues, reRoute))
                : NoMatch(request);
        }

        private Response<DownstreamRoute> NoMatch(HttpRequest request)
        {
            return new ErrorResponse<DownstreamRoute>(new UnableToFindDownstreamRouteError(request.Path, request.Method));
        }
    }
}
