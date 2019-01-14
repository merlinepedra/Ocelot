using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Ocelot.Configuration.Repository;
using Ocelot.DownstreamRouteFinder.Finder;
using Ocelot.Infrastructure.Extensions;
using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.Middleware.Multiplexer;

namespace Ocelot.DownstreamRouteFinder.Middleware
{
    public class DownstreamRouteFinderMiddleware : OcelotMiddleware
    {
        private readonly OcelotRequestDelegate _next;
        private readonly IDownstreamRouteProviderFactory _factory;
        private readonly IMultiplexer _multiplexer;

        public DownstreamRouteFinderMiddleware(OcelotRequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IDownstreamRouteProviderFactory downstreamRouteFinder,
            IMultiplexer multiplexer)
                :base(loggerFactory.CreateLogger<DownstreamRouteFinderMiddleware>())
        {
            _multiplexer = multiplexer;
            _next = next;
            _factory = downstreamRouteFinder;
        }

        public async Task Invoke(DownstreamContext context)
        {
            Logger.LogDebug($"Upstream url path is {context.HttpContext.Request.Path}");

            var provider = _factory.Get(context.Configuration);

            var downstreamRoute = provider.Get(context.HttpContext.Request, context.Configuration);

            if (downstreamRoute.IsError)
            {
                Logger.LogWarning($"{MiddlewareName} setting pipeline errors. IDownstreamRouteFinder returned {downstreamRoute.Errors.ToErrorString()}");

                SetPipelineError(context, downstreamRoute.Errors);
                return;
            }

            var downstreamPathTemplates = string.Join(", ", downstreamRoute.Data.ReRoute.DownstreamReRoute.Select(r => r.DownstreamPathTemplate.Value));

            Logger.LogDebug($"downstream templates are {downstreamPathTemplates}");

            context.UpstreamUrlValues = downstreamRoute.Data.UrlValues;

            await _multiplexer.Multiplex(context, downstreamRoute.Data.ReRoute, _next);
        }
    }
}
