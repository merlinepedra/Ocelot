namespace Ocelot.Placeholders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Infrastructure;
    using Infrastructure.Extensions;
    using Middleware;
    using Responses;

    public class PlaceholderProcessor : IPlaceholderProcessor
    {
        private readonly ImmutableDictionary<string, IPlaceholderProvider> _providers;
        private readonly IPlaceholderProvider _defaultProvider;

        public PlaceholderProcessor(IEnumerable<IPlaceholderProvider> providers)
        {
            _providers = providers.ToImmutableDictionary(k => k.PlaceholderProviderName);
            _defaultProvider = _providers["default"];
        }

        public Response<string> ProcessTemplate(DownstreamContext context, string template)
        {
            try
            {
                var result = new StringBuilder(template, template.Length * 2);
                foreach (Match match in template.MatchPlaceholders())
                {
                    var hasOp = match.Groups[2].Value.Length == 2;
                    var provider = hasOp ? LocalGetProvider(match.Groups[1].Value) : _defaultProvider;
                    result.Replace(match.Value,
                        provider.GetValues(context, hasOp ? match.Groups[3].Value : match.Groups[1].Value)
                            .First());
                }

                return new OkResponse<string>(result.ToString());
            }
            catch (Exception e)
            {
                return new ErrorResponse<string>(new CannotAddPlaceholderError(e.Message));
            }
        }

        public Response<IPlaceholderProvider> GetProvider(string providerName)
        {
            return new OkResponse<IPlaceholderProvider>(LocalGetProvider(providerName));
        }

        private IPlaceholderProvider LocalGetProvider(string providerName)
        {
            return _providers.TryGetValue(providerName, out var provider) ? provider : _defaultProvider;
        }
    }
}
