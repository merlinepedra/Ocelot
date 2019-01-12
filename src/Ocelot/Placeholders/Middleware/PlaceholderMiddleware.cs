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
        private readonly IPlaceholderProcessor _placeholderProcessor;

        public PlaceholderMiddleware(OcelotRequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IPlaceholderProcessor placeholderProcessor)
            :base(loggerFactory.CreateLogger<PlaceholderMiddleware>())
        {
            _next = next;
            _placeholderProcessor = placeholderProcessor;
        }

        public async Task Invoke(DownstreamContext context)
        {
            context.TemplatePlaceholderNameAndValues =
                _placeholderProcessor.GetPlaceholdersForTemplate(context,
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
