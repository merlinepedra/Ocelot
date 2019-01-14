namespace Ocelot.Middleware
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Configuration;
    using Errors;
    using Microsoft.AspNetCore.Http;
    using Ocelot.Request.Middleware;

    public class DownstreamContext
    {
        public DownstreamContext(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }

        public ImmutableDictionary<string, string> UpstreamUrlValues { get; set; } = ImmutableDictionary<string, string>.Empty;

        public HttpContext HttpContext { get; }

        public DownstreamReRoute DownstreamReRoute { get; set; }

        public DownstreamRequest DownstreamRequest { get; set; }

        public DownstreamResponse DownstreamResponse { get; set; }

        public List<Error> Errors { get; } = new List<Error>();

        public IInternalConfiguration Configuration { get; set; }

        public bool IsError => Errors.Count > 0;
    }
}
