namespace Ocelot.Placeholders
{
    using Middleware;
    using Responses;

    public interface IPlaceholderProcessor
    {
        /// <summary>
        /// This method processes a templated string in the current context
        /// </summary>
        /// <param name="context">The current downstream context</param>
        /// <param name="template">The template to process</param>
        /// <returns>The processed template</returns>
        Response<string> ProcessTemplate(DownstreamContext context, string template);

        /// <summary>
        /// This method retrieves the placeholder provider with the given name
        /// </summary>
        /// <param name="providerName">The name of the provider to retrieve</param>
        /// <returns>The specified provider or the default one if it is not found</returns>
        Response<IPlaceholderProvider> GetProvider(string providerName);
    }
}
