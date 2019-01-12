using System.Text.RegularExpressions;
using System.Threading;
using Ocelot.Responses;
using Ocelot.Values;

namespace Ocelot.DownstreamRouteFinder.UrlMatcher
{
    public class RegExUrlMatcher : IUrlPathToUrlTemplateMatcher
    {
        public Response<UrlMatch> Match(string upstreamUrlPath, string upstreamQueryString, UpstreamPathTemplate pathTemplate)
        {
            var matchPath = pathTemplate.ContainsQueryString
                ? $"{upstreamUrlPath}{upstreamQueryString}"
                : upstreamUrlPath;
            var match = pathTemplate.Pattern.Match(matchPath);

            return new OkResponse<UrlMatch>(new UrlMatch(match));
        }
    }
}
