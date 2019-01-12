using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Logging;
using Ocelot.Middleware;

namespace Ocelot.Placeholders.Middleware
{
    /// <summary>
    /// PlaceholderMiddleware ensures that all downstream placeholder values are set on the context.
    /// </summary>
    public class PlaceholderMiddleware : OcelotMiddleware
    {
        private readonly OcelotRequestDelegate _next;
        private readonly IPlaceholderFactory _placeholderFactory;

        public PlaceholderMiddleware(OcelotRequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IPlaceholderFactory placeholderFactory)
            :base(loggerFactory.CreateLogger<PlaceholderMiddleware>())
        {
            _next = next;
            _placeholderFactory = placeholderFactory;
        }

        public async Task Invoke(DownstreamContext context)
        {
            context.TemplatePlaceholderNameAndValues =
                _placeholderFactory.GetPlaceholdersForTemplate(context,
                    context.DownstreamReRoute.DownstreamPathTemplate.Value);
            
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception)
            {
                Logger.LogDebug("Exception calling next middleware, exception will be thrown to global handler");
                throw;
            }
        }
    }
}
