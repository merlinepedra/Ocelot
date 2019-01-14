namespace Ocelot.DownstreamUrlCreator.Middleware
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Logging;
    using Ocelot.Middleware;
    using Placeholders;
    using Values;

    public class DownstreamUrlCreatorMiddleware : OcelotMiddleware
    {
        private readonly OcelotRequestDelegate _next;
        private readonly IPlaceholderProcessor _processor;

        public DownstreamUrlCreatorMiddleware(OcelotRequestDelegate next,
            IOcelotLoggerFactory loggerFactory,
            IPlaceholderProcessor placeholderProcessor)
                :base(loggerFactory.CreateLogger<DownstreamUrlCreatorMiddleware>())
        {
            _next = next;
            _processor = placeholderProcessor;
        }

        public async Task Invoke(DownstreamContext context)
        {
            var response = _processor.ProcessTemplate(context, context.DownstreamReRoute.DownstreamPathTemplate.Value);

            if (response.IsError)
            {
                Logger.LogDebug("IDownstreamPathPlaceholderReplacer returned an error, setting pipeline error");

                SetPipelineError(context, response.Errors);
                return;
            }

            context.DownstreamRequest.Scheme = context.DownstreamReRoute.DownstreamScheme;
            var dsPath = new DownstreamPath(response.Data);

            if (ServiceFabricRequest(context))
            {
                var pathAndQuery = CreateServiceFabricUri(context, dsPath);
                context.DownstreamRequest.AbsolutePath = pathAndQuery.path;
                context.DownstreamRequest.Query = pathAndQuery.query;
            }
            else
            {

                if(ContainsQueryString(dsPath))
                {
                    context.DownstreamRequest.AbsolutePath = GetPath(dsPath);

                    if (string.IsNullOrEmpty(context.DownstreamRequest.Query))
                    {
                        context.DownstreamRequest.Query = GetQueryString(dsPath);
                    }
                    else
                    {
                        context.DownstreamRequest.Query += GetQueryString(dsPath).Replace('?', '&');
                    }
                }
                else
                {
                    RemoveQueryStringParametersThatHaveBeenUsedInTemplate(context);

                    context.DownstreamRequest.AbsolutePath = dsPath.Value;
                }
            }

            Logger.LogDebug($"Downstream url is {context.DownstreamRequest}");

            await _next.Invoke(context);
        }

        private static void RemoveQueryStringParametersThatHaveBeenUsedInTemplate(DownstreamContext context)
        {
            foreach (var keyValuePair in context.UpstreamUrlValues)
            {
                if (!context.DownstreamRequest.Query.Contains(keyValuePair.Key) ||
                    !context.DownstreamRequest.Query.Contains(keyValuePair.Value)) continue;
                var questionMarkOrAmpersand = context.DownstreamRequest.Query.IndexOf(keyValuePair.Key, StringComparison.Ordinal);
                context.DownstreamRequest.Query = context.DownstreamRequest.Query.Remove(questionMarkOrAmpersand - 1, 1);

                var rgx = new Regex($@"\b{keyValuePair.Key}={keyValuePair.Value}\b");
                context.DownstreamRequest.Query = rgx.Replace(context.DownstreamRequest.Query, "");

                if (!string.IsNullOrEmpty(context.DownstreamRequest.Query))
                {
                    context.DownstreamRequest.Query = '?' + context.DownstreamRequest.Query.Substring(1);
                }
            }
        }

        private string GetPath(DownstreamPath dsPath)
        {
            return dsPath.Value.Substring(0, dsPath.Value.IndexOf("?", StringComparison.Ordinal));
        }

        private string GetQueryString(DownstreamPath dsPath)
        {
            return dsPath.Value.Substring(dsPath.Value.IndexOf("?", StringComparison.Ordinal));
        }

        private bool ContainsQueryString(DownstreamPath dsPath)
        {
            return dsPath.Value.Contains("?");
        }

        private (string path, string query) CreateServiceFabricUri(DownstreamContext context, DownstreamPath dsPath)
        {
            var query = context.DownstreamRequest.Query;
            var serviceName = _processor.ProcessTemplate(context, context.DownstreamReRoute.ServiceName);
            var pathTemplate = $"/{serviceName.Data}{dsPath.Value}";
            return (pathTemplate, query);
        }

        private static bool ServiceFabricRequest(DownstreamContext context)
        {
            return context.Configuration.ServiceProviderConfiguration.Type?.ToLower() == "servicefabric" && context.DownstreamReRoute.UseServiceDiscovery;
        }
    }
}
