using Ocelot.Middleware.Pipeline;

namespace Ocelot.Placeholders.Middleware
{
    public static class PlaceholderMiddlewareExtensions
    {
        public static IOcelotPipelineBuilder UsePlaceholderMiddleware(this IOcelotPipelineBuilder builder)
        {
            return builder.UseMiddleware<PlaceholderMiddleware>();
        }
    }
}
