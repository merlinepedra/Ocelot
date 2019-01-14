namespace Ocelot.Placeholders
{
    using System.Collections.Generic;
    using DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Middleware;
    using Responses;

    public interface IPlaceholderProcessor
    {
        Response<string> ProcessTemplate(DownstreamContext context, string template);

        PlaceholderNameAndValue GetValueForMatch(DownstreamContext context, string match, bool trim = true);

        IEnumerable<PlaceholderNameAndValue> GetValuesForMatch(DownstreamContext context, string match,
            bool trim = true);

        List<PlaceholderNameAndValue> GetPlaceholdersForTemplate(DownstreamContext context, string template);

        IPlaceholderProvider GetProvider(string placeHolder);
    }
}
