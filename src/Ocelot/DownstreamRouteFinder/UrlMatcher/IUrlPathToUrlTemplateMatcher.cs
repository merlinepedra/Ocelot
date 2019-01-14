namespace Ocelot.DownstreamRouteFinder.UrlMatcher
{
    using Configuration;
    using Microsoft.AspNetCore.Http;
    using Responses;

    public interface IUrlPathToUrlTemplateMatcher
    {
        Response<DownstreamRoute> Match(HttpRequest request, ReRoute reRoute);
    }
}
