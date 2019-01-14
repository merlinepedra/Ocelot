namespace Ocelot.Placeholders
{
    using System.Collections.Generic;
    using Ocelot.Middleware;

    public interface IPlaceholderProvider
    {
        /// <summary>
        /// Gets the value that is used to match the provider to the method name defined in the template or other source
        /// </summary>
        string PlaceholderProviderName { get; }

        /// <summary>
        /// This gets all the values for this provider from the current context
        /// </summary>
        /// <param name="context">The current downstream context</param>
        /// <param name="value">The value for the provider to use</param>
        /// <returns>The list of results or an empty list if not found</returns>
        IEnumerable<string> GetValues(DownstreamContext context, string value);
    }
}
