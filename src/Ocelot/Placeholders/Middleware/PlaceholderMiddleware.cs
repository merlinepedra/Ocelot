using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ocelot.Logging;
using Ocelot.Middleware;

namespace Ocelot.Placeholders.Middleware
{
    /// <summary>
    /// PlaceholderMiddleware ensures that all downstream placeholder values are set on the context.
    /// </summary>
    public class PlaceholderMiddleware : OcelotMiddleware
    {
        private readonly Regex _placeholderPatternMatcher = new Regex(@"{[^{]*->[^}]*}", RegexOptions.Compiled | RegexOptions.Singleline);
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
            var matches = _placeholderPatternMatcher.Matches(context.DownstreamReRoute.DownstreamPathTemplate.Value);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    // TODO: Add values to the context... Logging now for test purposes.
                    // _placeholderFactory.Get(match.ToString()).DoStuff();
                    Logger.LogDebug($"Match found for placeholder {match}");
                    
                }
            }
            
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
